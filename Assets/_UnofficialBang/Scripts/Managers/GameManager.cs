using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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

        #endregion

        #region Public properties

        public static GameManager Instance { get; private set; }

        public List<CardData> Cards { get; private set; }

        public CardSpriteTable CardSpriteTable => cardSpriteTable;
        public ColorSettings ColorSettings => colorSettings;
        public AnimationSettings AnimationSettings => animationSettings;

        #endregion

        #region Events

        public UnityAction<BaseState> OnStateEnter { get; set; }
        public UnityAction<BaseState> OnStateExit { get; set; }

        public UnityAction<CardDealingEventData> CardDealing { get; set; }
        public UnityAction<RoleRevealingEventData> RoleRevealing { get; set; }

        public UnityAction<CardView> CardMouseOverEnter { get; set; }
        public UnityAction CardMouseOverExit { get; set; }

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
                CardDealing += OnCardDealing;
                RoleRevealing += OnRoleRevealing;
            }
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            if (PhotonNetwork.IsMasterClient)
            {
                CardDealing -= OnCardDealing;
                RoleRevealing -= OnRoleRevealing;
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

            var rolesDeck = roles.Where(r => r.IsRenegade || r.IsSceriff).ToList();
            var outlaws = roles.Where(r => r.IsOutlaw).Take(outlawCount);
            var deputies = roles.Where(r => r.IsDeputy).Take(deputyCount);

            rolesDeck.AddRange(outlaws);
            rolesDeck.AddRange(deputies);

            PhotonNetwork.CurrentRoom.RolesDeckCardIds = rolesDeck
                .Select(c => c.Id)
                .ToArray()
                .Shuffle();
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

                case PhotonEvent.CardDealing:
                    var cardDealingEventData = JsonConvert.DeserializeObject<CardDealingEventData>(json);
                    CardDealing?.Invoke(cardDealingEventData);
                    break;

                case PhotonEvent.RoleRevealing:
                    var roleRevealingEventData = JsonConvert.DeserializeObject<RoleRevealingEventData>(json);
                    RoleRevealing?.Invoke(roleRevealingEventData);
                    break;
            }
        }

        private void OnCardDealing(CardDealingEventData eventData)
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
                    var health = role.IsSceriff ? card.Health.Value + 1 : card.Health.Value;
                    player.MaxHealth = health;
                    player.CurrentHealth = health;
                    player.CharacterCardId = card.Id;
                    break;

                case CardClass.Role:
                    player.RoleCardId = card.Id;
                    break;
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            var players = PhotonNetwork.CurrentRoom.TurnPlayerIds.ToList();

            var card = Cards[eventData.CardId];
            if (card.Class == CardClass.Role && card.IsSceriff)
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

            string message = $"Player order: ";
            players.ForEach(p => message += p + ",");
            Debug.Log(message);
        }

        #endregion
    }
}