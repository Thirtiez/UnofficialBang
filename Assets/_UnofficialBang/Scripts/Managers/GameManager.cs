using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Thirties.UnofficialBang
{
    public class GameManager : MonoBehaviour, IOnEventCallback
    {
        #region Inspector fields

        [Header("FSM")]

        [SerializeField]
        private Animator fsm;

        [Header("Tables")]

        [SerializeField]
        private CardSpriteTable cardSpriteTable;

        [SerializeField]
        private CardDataTable baseCardDataTable;

        [Header("Settings")]

        [SerializeField]
        private ColorSettings colorSettings;

        [SerializeField]
        private AnimationSettings animationSettings;

#if BANG_DEBUG
        [Header("Debug")]

        [SerializeField]
        private DeckDebug deckDebug;
#endif

        #endregion

        #region Public properties

        public static GameManager Instance { get; private set; }

        public List<CardData> Cards { get; private set; }
        public BaseState CurrentState { get; set; }

        public CardSpriteTable CardSpriteTable => cardSpriteTable;
        public ColorSettings ColorSettings => colorSettings;
        public AnimationSettings AnimationSettings => animationSettings;

        public bool IsLocalPlayerTurn => PhotonNetwork.CurrentRoom.CurrentPlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
        public bool IsLocalPlayerTarget => PhotonNetwork.CurrentRoom.CurrentTargetId == PhotonNetwork.LocalPlayer.ActorNumber;

        #endregion

        #region Events

        public UnityAction<BaseState> StateEnter { get; set; }
        public UnityAction<BaseState> StateExit { get; set; }

        public UnityAction<DealingCardEventData> DealingCard { get; set; }
        public UnityAction<RevealingRoleEventData> RevealingRole { get; set; }
        public UnityAction<PlayingCardEventData> PlayingCard { get; set; }
        public UnityAction<TakingDamageEventData> TakingDamage { get; set; }
        public UnityAction<GainingHealthEventData> GainingHealth { get; set; }
        public UnityAction<DiscardingCardEventData> DiscardingCard { get; set; }
        public UnityAction<StealingCardEventData> StealingCard { get; set; }

        public UnityAction<CardHoverEnterEventData> CardHoverEnter { get; set; }
        public UnityAction CardHoverExit { get; set; }
        public UnityAction<SelectingCardEventData> CardSelected { get; set; }
        public UnityAction CardCanceled { get; set; }

        public UnityAction<CardPickerEnterEventData> CardPickerEnter { get; set; }
        public UnityAction<CardPickerExitEventData> CardPickerExit { get; set; }

        #endregion

        #region Monobehaviour methods

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;

                Cards = baseCardDataTable.GetAll().OrderBy(x => x.Id).ToList();
            }
        }

        private void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (PhotonNetwork.IsMasterClient)
            {
                DealingCard += OnDealingCard;
                RevealingRole += OnRevealingRole;
                PlayingCard += OnPlayingCard;
                TakingDamage += OnTakingDamage;
                GainingHealth += OnGainingHealth;
                DiscardingCard += OnDiscardingCard;
                StealingCard += OnStealingCard;
            }
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            if (PhotonNetwork.IsMasterClient)
            {
                DealingCard -= OnDealingCard;
                RevealingRole -= OnRevealingRole;
                PlayingCard -= OnPlayingCard;
                TakingDamage -= OnTakingDamage;
                GainingHealth -= OnGainingHealth;
                DiscardingCard -= OnDiscardingCard;
                StealingCard -= OnStealingCard;
            }
        }

        #endregion

        #region Public methods

        public void SendEvent(byte gameEvent, BaseEventData eventData = null, RaiseEventOptions raiseEventOptions = null, SendOptions? sendOptions = null)
        {
            var json = eventData != null ? JsonConvert.SerializeObject(eventData) : "";

            Debug.Log($"<color=cyan>Event {gameEvent} sent with data: {json}</color>");

            raiseEventOptions = raiseEventOptions ?? new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = sendOptions ?? SendOptions.SendReliable;

            PhotonNetwork.RaiseEvent(gameEvent, json, raiseEventOptions, sendOptions.Value);
        }

        public void InitializePlayer()
        {
            PhotonNetwork.LocalPlayer.ClearCustomProperties();
        }

        public void InitializeRoom()
        {
            PhotonNetwork.CurrentRoom.ClearCustomProperties();

            PhotonNetwork.CurrentRoom.TurnPlayerIds = PhotonNetwork.PlayerList
                .Select(p => p.ActorNumber)
                .OrderBy(p => p)
                .ToArray();

#if BANG_DEBUG
            PhotonNetwork.CurrentRoom.MainDeckCardIds = deckDebug.MainCards
                .Select(c => c.Id)
                .ToArray();

            PhotonNetwork.CurrentRoom.CharactersDeckCardIds = deckDebug.CharacterCards
                .Select(c => c.Id)
                .ToArray();

            PhotonNetwork.CurrentRoom.RolesDeckCardIds = deckDebug.RoleCards
                .Take(PhotonNetwork.CurrentRoom.PlayerCount)
                .Select(c => c.Id)
                .ToArray();
#else
            PhotonNetwork.CurrentRoom.MainDeckCardIds = Cards
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .Select(c => c.Id)
                .ToArray()
                .Shuffle();

            PhotonNetwork.CurrentRoom.CharactersDeckCardIds = Cards
                .Where(c => c.Class == CardClass.Character)
                .Select(c => c.Id)
                .ToArray()
                .Shuffle();

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int outlawCount = playerCount == 2 ? 0 : playerCount / 2;
            int deputyCount = playerCount > 6 ? 2 : playerCount > 4 ? 1 : 0;

            var roles = Cards
                .Where(c => c.Class == CardClass.Role)
                .ToList();

            var rolesDeck = roles.Where(r => r.Effect == CardEffect.Renegade || r.Effect == CardEffect.Sceriff).ToList();
            var outlaws = roles.Where(r => r.Effect == CardEffect.Outlaw).Take(outlawCount);
            var deputies = roles.Where(r => r.Effect == CardEffect.Deputy).Take(deputyCount);

            rolesDeck.AddRange(outlaws);
            rolesDeck.AddRange(deputies);

            PhotonNetwork.CurrentRoom.RolesDeckCardIds = rolesDeck
                .Select(c => c.Id)
                .ToArray()
                .Shuffle();
#endif
        }

        public CardData DrawPlayingCard()
        {
            var deck = PhotonNetwork.CurrentRoom.MainDeckCardIds;
            var card = DrawCard(ref deck);
            PhotonNetwork.CurrentRoom.MainDeckCardIds = deck;
            return card;
        }

        public CardData DrawDiscardedCard()
        {
            var deck = PhotonNetwork.CurrentRoom.DiscardDeckCardIds;
            var card = DrawCard(ref deck);
            PhotonNetwork.CurrentRoom.DiscardDeckCardIds = deck;
            return card;
        }

        public CardData DrawCharacter()
        {
            var deck = PhotonNetwork.CurrentRoom.CharactersDeckCardIds;
            var card = DrawCard(ref deck);
            PhotonNetwork.CurrentRoom.CharactersDeckCardIds = deck;
            return card;
        }

        public CardData DrawRole()
        {
            var deck = PhotonNetwork.CurrentRoom.RolesDeckCardIds;
            var card = DrawCard(ref deck);
            PhotonNetwork.CurrentRoom.RolesDeckCardIds = deck;
            return card;
        }

        public int GetNextLivingPlayerId(int? fromPlayer = null)
        {
            var turnPlayerIds = PhotonNetwork.CurrentRoom.TurnPlayerIds.ToList();
            int index = turnPlayerIds.IndexOf(fromPlayer ?? PhotonNetwork.CurrentRoom.CurrentPlayerId);

            do
            {
                index = (index + 1) % turnPlayerIds.Count;
            } while (!PhotonNetwork.CurrentRoom.GetPlayer(turnPlayerIds[index]).IsAlive);

            return turnPlayerIds[index];
        }

        #endregion

        #region Private methods

        private CardData DrawCard(ref int[] deckIds)
        {
            if (deckIds.Length > 0)
            {
                var cardId = deckIds[0];
                deckIds = deckIds.Skip(1).ToArray();
                return Cards[cardId];
            }

            return null;
        }

        #endregion

        #region Event handlers

        public void OnEvent(EventData photonEvent)
        {
            var json = photonEvent.CustomData as string;

            Debug.Log($"<color=cyan>Event {photonEvent.Code} received with data: {json}</color>");

            switch (photonEvent.Code)
            {
                case PhotonEvent.ChangingState:
                    var changingStateEventData = JsonConvert.DeserializeObject<ChangingStateEventData>(json);
                    fsm.SetTrigger(changingStateEventData.Trigger);
                    break;

                case PhotonEvent.DealingCard:
                    var cardDealingEventData = JsonConvert.DeserializeObject<DealingCardEventData>(json);
                    DealingCard?.Invoke(cardDealingEventData);
                    break;

                case PhotonEvent.RevealingRole:
                    var roleRevealingEventData = JsonConvert.DeserializeObject<RevealingRoleEventData>(json);
                    RevealingRole?.Invoke(roleRevealingEventData);
                    break;

                case PhotonEvent.PlayingCard:
                    var cardPlayingEventData = JsonConvert.DeserializeObject<PlayingCardEventData>(json);
                    PlayingCard?.Invoke(cardPlayingEventData);
                    break;

                case PhotonEvent.TakingDamage:
                    var takingDamageEventData = JsonConvert.DeserializeObject<TakingDamageEventData>(json);
                    TakingDamage?.Invoke(takingDamageEventData);
                    break;

                case PhotonEvent.GainingHealth:
                    var gainingHealthEventData = JsonConvert.DeserializeObject<GainingHealthEventData>(json);
                    GainingHealth?.Invoke(gainingHealthEventData);
                    break;

                case PhotonEvent.DiscardingCard:
                    var discardingCardEventData = JsonConvert.DeserializeObject<DiscardingCardEventData>(json);
                    DiscardingCard?.Invoke(discardingCardEventData);
                    break;

                case PhotonEvent.StealingCard:
                    var stealingCardEventData = JsonConvert.DeserializeObject<StealingCardEventData>(json);
                    StealingCard?.Invoke(stealingCardEventData);
                    break;
            }
        }

        private void OnDealingCard(DealingCardEventData eventData)
        {
            var card = Cards[eventData.CardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);

            switch (card.Class)
            {
                case CardClass.Brown:
                case CardClass.Blue:
                    player.HandCardIds = player.HandCardIds.AppendWith(card.Id).ToArray();
                    break;

                case CardClass.Character:
                    var role = Cards[player.RoleCardId];
                    var health = role.Effect == CardEffect.Sceriff ? card.Health.Value + 1 : card.Health.Value;
                    player.MaxHealth = health;
                    player.CurrentHealth = health;
                    player.CharacterCardId = card.Id;
                    player.Range = card.Effect == CardEffect.Mustang ? 2 : 1;
                    break;

                case CardClass.Role:
                    player.RoleCardId = card.Id;
                    break;
            }
        }

        private void OnRevealingRole(RevealingRoleEventData eventData)
        {
            var players = PhotonNetwork.CurrentRoom.TurnPlayerIds.ToList();

            var card = Cards[eventData.CardId];
            if (card.Class == CardClass.Role && card.Effect == CardEffect.Sceriff)
            {
                var player = players.SingleOrDefault(p => p == eventData.PlayerId);
                var rotateAmount = players.IndexOf(player);

                for (int i = 0; i < rotateAmount; i++)
                {
                    var last = players[players.Count - 1];
                    players.RemoveAt(players.Count - 1);
                    players.Insert(0, last);
                }
            }

            PhotonNetwork.CurrentRoom.TurnPlayerIds = players.ToArray();
        }

        private void OnPlayingCard(PlayingCardEventData eventData)
        {
            var instigator = PhotonNetwork.CurrentRoom.GetPlayer(eventData.InstigatorId);
            var newHandCardIds = instigator.HandCardIds.Where(c => c != eventData.CardId).ToArray();
            instigator.HandCardIds = newHandCardIds;
        }

        private void OnTakingDamage(TakingDamageEventData eventData)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            player.CurrentHealth = Math.Max(0, player.CurrentHealth - eventData.Amount);

            if (player.CurrentHealth == 0)
            {
                //TODO elimination
            }
        }

        private void OnGainingHealth(GainingHealthEventData eventData)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            player.CurrentHealth = Math.Min(player.CurrentHealth + 1, player.MaxHealth);
        }

        private void OnDiscardingCard(DiscardingCardEventData eventData)
        {
            var target = PhotonNetwork.CurrentRoom.GetPlayer(eventData.TargetId);
            var card = Cards[eventData.CardId];

            if (eventData.IsFromHand)
            {
                target.HandCardIds = target.HandCardIds.Where(c => c != card.Id).ToArray();
            }
            else
            {
                target.BoardCardIds = target.BoardCardIds.Where(c => c != card.Id).ToArray();
                switch (card.Effect)
                {
                    case CardEffect.Scope:
                        target.Range -= 1;
                        break;

                    case CardEffect.Mustang:
                        target.BonusDistance -= 1;
                        break;

                    case CardEffect.Weapon:
                        target.Range -= card.EffectValue.Value;
                        break;
                }
            }
        }

        private void OnStealingCard(StealingCardEventData eventData)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            var target = PhotonNetwork.CurrentRoom.GetPlayer(eventData.TargetId);
            var card = Cards[eventData.CardId];

            if (eventData.IsFromHand)
            {
                target.HandCardIds = target.HandCardIds.Where(c => c != card.Id).ToArray();
            }
            else
            {
                target.BoardCardIds = target.BoardCardIds.Where(c => c != card.Id).ToArray();
            }

            player.HandCardIds = player.HandCardIds.AppendWith(card.Id).ToArray();
        }

        #endregion
    }
}