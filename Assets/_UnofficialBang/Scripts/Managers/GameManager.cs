using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
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

        public Player CurrentPlayer { get; set; }

        public List<CardData> Cards { get; private set; }
        public List<Player> Players { get; private set; }

        public List<CardData> PlayerHand { get; private set; }
        public List<CardData> PlayerBoard { get; private set; }
        public CardData PlayerCharacter { get; private set; }
        public CardData PlayerRole { get; private set; }

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

        #region Private fields

        private List<CardData> _mainDeck;
        private List<CardData> _rolesDeck;
        private List<CardData> _charactersDeck;
        private List<CardData> _discardPile;

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
                Players = PhotonNetwork.PlayerList.OrderBy(x => x.ActorNumber).ToList();
            }
        }

        private void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);

            CardDealing += OnCardDealing;
            RoleRevealing += OnRoleRevealing;
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            CardDealing -= OnCardDealing;
            RoleRevealing -= OnRoleRevealing;
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

        public void SetPlayerProperties(PlayerCustomProperties customProperties = null)
        {
            customProperties = customProperties ?? new PlayerCustomProperties();

            var json = JsonConvert.SerializeObject(customProperties);
            var hashtable = new Hashtable();
            hashtable["json"] = json;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        }

        public PlayerCustomProperties GetPlayerProperties(Player player = null)
        {
            player = player ?? PhotonNetwork.LocalPlayer;

            if (player.CustomProperties.ContainsKey("json"))
            {
                var json = (string)player.CustomProperties["json"];
                var customProperties = JsonConvert.DeserializeObject<PlayerCustomProperties>(json);
                return customProperties;
            }

            return new PlayerCustomProperties();
        }

        public void InitializePlayer()
        {
            PlayerHand = new List<CardData>();
            PlayerBoard = new List<CardData>();
            PlayerCharacter = null;
            PlayerRole = null;

            SetPlayerProperties();
        }

        public void InitializeDecks()
        {
            _mainDeck = Cards
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .ToList()
                .Shuffle();

            _charactersDeck = Cards
                .Where(c => c.Class == CardClass.Character)
                .ToList()
                .Shuffle();

            _discardPile = new List<CardData>();
            _rolesDeck = new List<CardData>();

            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int outlawCount = playerCount == 2 ? 0 : playerCount / 2;
            int deputyCount = playerCount > 6 ? 2 : playerCount > 4 ? 1 : 0;

            var roles = Cards
                .Where(c => c.Class == CardClass.Role)
                .ToList();

            var baseRoles = roles.Where(r => r.IsRenegade || r.IsSceriff);
            var outlaws = roles.Where(r => r.IsOutlaw).Take(outlawCount);
            var deputies = roles.Where(r => r.IsDeputy).Take(deputyCount);

            _rolesDeck.AddRange(baseRoles);
            _rolesDeck.AddRange(outlaws);
            _rolesDeck.AddRange(deputies);

            _rolesDeck = _rolesDeck.Shuffle();
        }

        public CardData DrawPlayingCard()
        {
            return DrawCard(_mainDeck);
        }

        public CardData DrawDiscardedCard()
        {
            return DrawCard(_discardPile);
        }

        public CardData DrawCharacter()
        {
            return DrawCard(_charactersDeck);
        }

        public CardData DrawRole()
        {
            return DrawCard(_rolesDeck);
        }

        #endregion

        #region Private methods

        private CardData DrawCard(List<CardData> deck)
        {
            CardData card = null;
            if (deck.Count > 0)
            {
                card = deck[0];
                deck.RemoveAt(0);
            }

            return card;
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

            if (PhotonNetwork.LocalPlayer.ActorNumber == eventData.PlayerId)
            {
                var customProperties = GetPlayerProperties(PhotonNetwork.LocalPlayer);
                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:

                        PlayerHand.Add(card);

                        customProperties.HandCount = PlayerHand.Count;
                        SetPlayerProperties(customProperties);
                        break;

                    case CardClass.Character:

                        PlayerCharacter = card;

                        customProperties.MaxHealth = card.Health.Value;
                        if (PlayerRole.IsSceriff)
                        {
                            customProperties.MaxHealth++;
                        }
                        customProperties.CurrentHealth = customProperties.MaxHealth;
                        SetPlayerProperties(customProperties);
                        break;

                    case CardClass.Role:

                        PlayerRole = card;
                        break;
                }

            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            var card = Cards[eventData.CardId];

            if (card.Class == CardClass.Role && card.IsSceriff)
            {
                var player = Players.SingleOrDefault(p => p.ActorNumber == eventData.PlayerId);
                var rotateAmount = Players.IndexOf(player);

                for (int i = 0; i < rotateAmount; i++)
                {
                    var last = Players[Players.Count - 1];
                    Players.RemoveAt(Players.Count - 1);
                    Players.Insert(0, last);
                }
            }
        }

        #endregion
    }
}