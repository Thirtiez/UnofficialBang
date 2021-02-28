using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Thirties.UnofficialBang
{
    public class GameManager : MonoBehaviour, IOnEventCallback
    {
        #region Inspector fields

        [Header("FSM")]

        [SerializeField]
        private Animator fsm;

        [Header("Scriptable objects")]

        [SerializeField]
        private CardSpriteTable cardSpriteTable;

        [SerializeField]
        private CardDataTable baseCardDataTable;

        [Header("UI")]

        [SerializeField]
        private GameLogUI gameLog;

        [SerializeField]
        private GameObject exitModal;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private Button cancelExitButton;

        [SerializeField]
        private Button confirmExitButton;

        #endregion

        #region Public properties

        public static GameManager Instance { get; private set; }

        public BaseState CurrentState { get; set; }
        public List<CardData> Cards { get; private set; }
        public PlayerCustomProperties PlayerProperties { get; private set; }
        public List<Player> Players { get; private set; }

        public CardSpriteTable CardSpriteTable => cardSpriteTable;
        public GameLogUI GameLog => gameLog;

        #endregion

        #region Events

        public UnityAction<CardDealingEventData> CardDealing { get; set; }
        public UnityAction<RoleRevealingEventData> RoleRevealing { get; set; }
        public UnityAction CharactersDealt { get; set; }
        public UnityAction CardsDealt { get; set; }

        #endregion

        #region Private fields

        private List<CardData> _mainDeck;
        private List<CardData> _rolesDeck;
        private List<CardData> _charactersDeck;
        private List<CardData> _discardPile;

        private List<CardData> _playerHand;
        private List<CardData> _playerBoard;
        private CardData _playerCharacter;
        private CardData _playerRole;

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

            exitButton.onClick.AddListener(OnExitButtonClicked);
            cancelExitButton.onClick.AddListener(OnCancelExitButtonClicked);
            confirmExitButton.onClick.AddListener(OnConfirmExitButtonClicked);

            exitButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
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

        public void SetPlayerProperties()
        {
            var json = JsonConvert.SerializeObject(PlayerProperties);
            var hashtable = new Hashtable();
            hashtable["json"] = json;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        }

        public PlayerCustomProperties GetPlayerProperties(Player player)
        {
            var actualPlayer = PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber);
            var json = (string)actualPlayer.CustomProperties["json"];
            var customProperties = JsonConvert.DeserializeObject<PlayerCustomProperties>(json);
            return customProperties;
        }

        public void InitializePlayer()
        {
            _playerHand = new List<CardData>();
            _playerBoard = new List<CardData>();
            _playerCharacter = null;
            _playerRole = null;

            PlayerProperties = new PlayerCustomProperties();
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
            int outlawCount = playerCount / 2;
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
            if (_rolesDeck.Count > 0)
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
                case PhotonEvent.CardDealing:
                    var cardDealingEventData = JsonConvert.DeserializeObject<CardDealingEventData>(json);
                    CardDealing?.Invoke(cardDealingEventData);
                    break;

                case PhotonEvent.RoleRevealing:
                    var roleRevealingEventData = JsonConvert.DeserializeObject<RoleRevealingEventData>(json);
                    RoleRevealing?.Invoke(roleRevealingEventData);
                    break;

                case PhotonEvent.CharactersDealt:
                    CharactersDealt?.Invoke();
                    break;

                case PhotonEvent.CardsDealt:
                    CardsDealt?.Invoke();
                    break;
            }
        }

        private void OnCardDealing(CardDealingEventData eventData)
        {
            var card = Cards[eventData.CardId];

            if (PhotonNetwork.LocalPlayer.ActorNumber == eventData.PlayerId)
            {
                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:
                        _playerHand.Add(card);

                        PlayerProperties.HandCount = _playerHand.Count;
                        break;

                    case CardClass.Character:
                        _playerCharacter = card;

                        PlayerProperties.MaxHealth = card.Health.Value;
                        if (_playerRole.IsSceriff)
                        {
                            PlayerProperties.MaxHealth++;
                        }
                        PlayerProperties.CurrentHealth = PlayerProperties.MaxHealth;

                        break;

                    case CardClass.Role:
                        _playerRole = card;
                        break;
                }

                SetPlayerProperties();
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

        private void OnExitButtonClicked()
        {
            exitModal.SetActive(true);
        }

        private void OnCancelExitButtonClicked()
        {
            exitModal.SetActive(false);
        }

        private void OnConfirmExitButtonClicked()
        {
            PhotonNetwork.LoadLevel("Main");
        }

        #endregion
    }
}