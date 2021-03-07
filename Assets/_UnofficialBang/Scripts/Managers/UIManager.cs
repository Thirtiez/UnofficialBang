using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class UIManager : MonoBehaviour
    {
        #region Inspector fields

        [Header("Game Log")]

        [SerializeField]
        private GameLogUI gameLog;

        [Header("Exit Modal")]

        [SerializeField]
        private GameObject exitModal;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private Button cancelExitButton;

        [SerializeField]
        private Button confirmExitButton;

        [Header("Card Zoom")]

        [SerializeField]
        private RectTransform cardZoom;

        [SerializeField]
        private Image cardZoomImage;

        [SerializeField]
        private Image highlightZoomImage;

        [Header("Header")]

        [SerializeField]
        private GameObject header;

        [SerializeField]
        private TMP_Text headerText;

        [Header("Command Section")]

        [SerializeField]
        private GameObject commandSection;

        [SerializeField]
        private TMP_Text instructionsText;

        [SerializeField]
        private Transform commandsContainer;

        [SerializeField]
        private CommandElementUI commandElementPrefab;

        #endregion

        #region Private fields

        private GameManager _gameManager;

        #endregion

        #region Monobehaviour methods

        private void Start()
        {
            _gameManager = GameManager.Instance;

            PhotonNetwork.AddCallbackTarget(this);

            _gameManager.OnStateEnter += OnStateEnter;
            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
            _gameManager.CardMouseOverEnter += OnCardMouseOverEnter;
            _gameManager.CardMouseOverExit += OnCardMouseOverExit;

            exitButton.onClick.AddListener(OnExitButtonClicked);
            cancelExitButton.onClick.AddListener(OnCancelExitButtonClicked);
            confirmExitButton.onClick.AddListener(OnConfirmExitButtonClicked);

            highlightZoomImage.color = _gameManager.ColorSettings.CardHighlight;

            cardZoom.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            exitModal.SetActive(false);
            header.SetActive(true);
            commandSection.SetActive(false);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            _gameManager.OnStateEnter -= OnStateEnter;
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
            _gameManager.CardMouseOverEnter -= OnCardMouseOverEnter;
            _gameManager.CardMouseOverExit -= OnCardMouseOverExit;
        }

        #endregion

        #region Game event handlers

        private void OnStateEnter(BaseState state)
        {
            if (state is PreparationState)
            {
                headerText.text = "Preparazione...";
            }
            else if (state is TurnState)
            {
                if (_gameManager.IsLocalPlayerTurn)
                {
                    headerText.text = $"È il <b><color=#{_gameManager.ColorSettings.PlayerColor}>tuo</color></b> turno";

                    if (state is CardSelectionState)
                    {
                        instructionsText.text = "Gioca una carta o passa";

                        var passCommand = Instantiate(commandElementPrefab, commandsContainer);
                        passCommand.Configure("Passa", () =>
                        {
                            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.DiscardPhase });
                        });

                        commandSection.SetActive(true);
                    }
                    else
                    {
                        commandSection.SetActive(false);
                    }
                }
                else
                {
                    headerText.text = $"È il turno di <b><color=#{_gameManager.ColorSettings.PlayerColor}>{PhotonNetwork.CurrentRoom.CurrentPlayer.NickName}</color></b>";
                }
            }
        }

        private void OnCardDealing(CardDealingEventData eventData)
        {
            if (eventData.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var card = _gameManager.Cards[eventData.CardId];

                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:
                        gameLog.Log("Hai pescato la carta {0}", card);
                        break;

                    case CardClass.Character:
                        gameLog.Log("Hai pescato il personaggio {0}", card);
                        break;

                    case CardClass.Role:
                        gameLog.Log("Hai pescato il ruolo {0}", card);
                        break;
                }
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string message = card.IsSceriff ? "{1} è lo {0}!" : "{1} era un {0}!";

            gameLog.Log(message, card, player);
        }

        #endregion

        #region Input event handlers

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
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.LoadLevel("Main");
        }

        private void OnCardMouseOverEnter(CardMouseOverEnterEventData eventData)
        {
            highlightZoomImage.gameObject.SetActive(eventData.IsPlayable);
            cardZoomImage.sprite = _gameManager.CardSpriteTable.Get(eventData.CardView.CardData.Sprite);

            var screenPosition = Camera.main.WorldToScreenPoint(eventData.CardView.transform.position);
            //bool left = screenPosition.x < Screen.width / 2;
            bool bottom = screenPosition.y < Screen.height / 2;
            var anchor = new Vector2(0.5f, bottom ? 0 : 1);
            cardZoom.anchorMin = anchor;
            cardZoom.anchorMax = anchor;
            cardZoom.pivot = anchor;
            cardZoom.transform.position = screenPosition;

            cardZoom.gameObject.SetActive(true);
        }

        private void OnCardMouseOverExit()
        {
            cardZoomImage.sprite = null;

            cardZoom.gameObject.SetActive(false);
        }

        #endregion
    }
}