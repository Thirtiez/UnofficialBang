using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class GameManager : MonoBehaviour, IOnEventCallback
    {
        [Header("FSM")]
        [SerializeField]
        private Animator fsm;

        [Header("Databases")]
        [SerializeField]
        private CardSpriteDatabase cardSpriteDatabase;

        [Header("Sets")]
        [SerializeField]
        private CardSetData baseSet;

        public static GameManager Instance { get; private set; }

        public List<Player> Players { get; private set; }

        private List<CardData> _mainDeck;
        private List<CardData> _rolesDeck;
        private List<CardData> _charactersDeck;
        private List<CardData> _discardPile;
        private List<CardData> _playerHand;
        private List<CardData> _playerBoard;
        private CardData _playerCharacter;
        private CardData _playerRole;

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
            }
        }

        private void SendEvent(byte gameEvent, BaseEventData eventData, ReceiverGroup receivers = ReceiverGroup.All)
        {
            var options = new RaiseEventOptions { Receivers = receivers };
            PhotonNetwork.RaiseEvent(gameEvent, eventData, options, SendOptions.SendReliable);
        }

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

                    SendEvent(GameEvent.DealCard, new DealCardEventData { Card = card, Player = player });
                    break;
            }
        }

        #region Photon event handlers

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case GameEvent.DealCard:
                    var eventData = photonEvent.CustomData as DealCardEventData;
                    OnCardDealt(eventData.Card, eventData.Player);
                    break;
            }
        }

        private void OnCardDealt(CardData card, Player player)
        {
            if (PhotonNetwork.LocalPlayer == player)
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

