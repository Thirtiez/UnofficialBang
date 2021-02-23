using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public List<Player> Players { get; private set; }
        public List<CardData> Cards { get; private set; }

        public CardSpriteTable CardSpriteDatabase => cardSpriteTable;
        public GameLogUI GameLog => gameLog;

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

                Players = PhotonNetwork.PlayerList.ToList();
                Cards = baseCardDataTable.Records.ToList();
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region Public methods

        public void InitializeDecks()
        {
            _playerHand = new List<CardData>();
            _playerBoard = new List<CardData>();
            _playerCharacter = null;
            _playerRole = null;

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            _mainDeck = baseCardDataTable.Records
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .ToList()
                .Shuffle();

            _charactersDeck = baseCardDataTable.Records
                .Where(c => c.Class == CardClass.Character)
                .ToList()
                .Shuffle();

            _rolesDeck = baseCardDataTable.Records
                .Where(c => c.Class == CardClass.Role)
                .ToList()
                .Shuffle();

            _discardPile = new List<CardData>();
        }

        public void DealCard(Player player, DeckClass deckClass)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            switch (deckClass)
            {
                case DeckClass.Main:
                    break;

                case DeckClass.Discard:
                    break;

                case DeckClass.Character:
                    break;

                case DeckClass.Role:
                    var card = _rolesDeck[0];
                    _rolesDeck.RemoveAt(0);

                    SendEvent(GameEvent.DealCard, new DealCardEventData { CardId = card.Id, PlayerId = player.ActorNumber });
                    break;
            }
        }

        #endregion

        #region Event methods and handlers

        private void SendEvent(byte gameEvent, BaseEventData eventData, RaiseEventOptions raiseEventOptions = null, SendOptions? sendOptions = null)
        {
            var json = JsonConvert.SerializeObject(eventData);

            Debug.Log($"<color=cyan>Event {gameEvent} sent with data: {json}</color>");

            raiseEventOptions = raiseEventOptions ?? new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = sendOptions ?? SendOptions.SendReliable;

            PhotonNetwork.RaiseEvent(gameEvent, json, raiseEventOptions, sendOptions.Value);
        }

        public void OnEvent(EventData photonEvent)
        {
            var json = photonEvent.CustomData as string;

            Debug.Log($"<color=cyan>Event {photonEvent.Code} received with data: {json}</color>");

            switch (photonEvent.Code)
            {
                case GameEvent.DealCard:
                    var eventData = JsonConvert.DeserializeObject<DealCardEventData>(json);
                    OnCardDealt(eventData.CardId, eventData.PlayerId);
                    break;
            }
        }

        private void OnCardDealt(int cardId, int playerId)
        {
            var card = baseCardDataTable.Records[cardId];

            if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
            {

                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:
                        playerView.DealPlayingCard(card);

                        _playerHand.Add(card);
                        break;

                    case CardClass.Character:
                        playerView.DealCharacter(card);

                        _playerCharacter = card;
                        break;

                    case CardClass.Role:
                        playerView.DealRole(card);

                        _playerRole = card;
                        break;
                }
            }
            else
            {

            }
        }

        #endregion
    }
}