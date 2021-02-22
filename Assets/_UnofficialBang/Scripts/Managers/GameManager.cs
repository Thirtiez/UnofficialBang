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

        [Header("Databases")]
        [SerializeField]
        private CardSpriteDatabase cardSpriteDatabase;

        [Header("Sets")]
        [SerializeField]
        private CardSetData baseSet;

        [Header("UI")]
        [SerializeField]
        private GameLog gameLog;
        public GameLog GameLog => gameLog;

        #endregion

        #region Public properties

        public static GameManager Instance { get; private set; }

        public List<Player> Players { get; private set; }
        public List<CardData> Cards { get; private set; }

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
                Cards = baseSet.Cards.ToList();
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

            _mainDeck = baseSet.Cards
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .ToList()
                .Shuffle();

            _charactersDeck = baseSet.Cards
                .Where(c => c.Class == CardClass.Character)
                .ToList()
                .Shuffle();

            _rolesDeck = baseSet.Cards
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

            Debug.Log($"Event {gameEvent} sent with data: {json}");

            raiseEventOptions = raiseEventOptions ?? new RaiseEventOptions { Receivers = ReceiverGroup.All };
            sendOptions = sendOptions ?? SendOptions.SendReliable;

            PhotonNetwork.RaiseEvent(gameEvent, json, raiseEventOptions, sendOptions.Value);
        }

        public void OnEvent(EventData photonEvent)
        {
            var json = photonEvent.CustomData as string;

            Debug.Log($"Event {photonEvent.Code} received with data: {json}");

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
            var card = baseSet.Cards[cardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);

            gameLog.Log("Dealt {0} to {1}", card, player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
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
            else
            {

            }
        }

        #endregion
    }
}

