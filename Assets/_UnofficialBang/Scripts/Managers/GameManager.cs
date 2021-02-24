using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
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

        [Header("Scriptable objects")]

        [SerializeField]
        private CardSpriteTable cardSpriteTable;

        [SerializeField]
        private CardDataTable baseCardDataTable;

        [Header("Views")]

        [SerializeField]
        private PlayerView playerView;

        [Header("UI")]

        [SerializeField]
        private GameLogUI gameLog;

        #endregion

        #region Public properties

        public static GameManager Instance { get; private set; }

        public List<CardData> Cards { get; private set; }

        public CardSpriteTable CardSpriteTable => cardSpriteTable;
        public GameLogUI GameLog => gameLog;

        #endregion

        #region Events

        public UnityAction<CardDealingEventData> CardDealing { get; set; }
        public UnityAction<RoleRevealingEventData> RoleRevealing { get; set; }
        public UnityAction<CharacterRevealingEventData> CharacterRevealing { get; set; }

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

                Cards = baseCardDataTable.GetAll();
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            CardDealing += OnCardDealing;
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            CardDealing -= OnCardDealing;
        }

        #endregion

        #region Public methods

        public void SendEvent(byte gameEvent, BaseEventData eventData, RaiseEventOptions raiseEventOptions = null, SendOptions? sendOptions = null)
        {
            var json = JsonConvert.SerializeObject(eventData);

            Debug.Log($"<color=cyan>Event {gameEvent} sent with data: {json}</color>");

            raiseEventOptions = raiseEventOptions ?? new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = sendOptions ?? SendOptions.SendReliable;

            PhotonNetwork.RaiseEvent(gameEvent, json, raiseEventOptions, sendOptions.Value);
        }

        public void InitializePlayer()
        {
            _playerHand = new List<CardData>();
            _playerBoard = new List<CardData>();
            _playerCharacter = null;
            _playerRole = null;
        }

        public void InitializeDecks()
        {
            _mainDeck = baseCardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .ToList()
                .Shuffle();

            _charactersDeck = baseCardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Character)
                .ToList()
                .Shuffle();

            _rolesDeck = baseCardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Role)
                .ToList()
                .Shuffle();

            _discardPile = new List<CardData>();
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

                case PhotonEvent.CharacterRevealing:
                    var characterRevealingEventData = JsonConvert.DeserializeObject<CharacterRevealingEventData>(json);
                    CharacterRevealing?.Invoke(characterRevealingEventData);
                    break;
            }
        }

        private void OnCardDealing(CardDealingEventData eventData)
        {
            var card = baseCardDataTable.Get(eventData.CardId);

            if (PhotonNetwork.LocalPlayer.ActorNumber == eventData.PlayerId)
            {
                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:
                        _playerHand.Add(card);
                        break;

                    case CardClass.Character:
                        _playerCharacter = card;
                        break;

                    case CardClass.Role:
                        _playerRole = card;
                        break;
                }
            }
        }

        #endregion
    }
}