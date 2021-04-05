using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        private Transform commandsContainer;

        [SerializeField]
        private CommandElementUI commandElementPrefab;

        #endregion

        #region Private fields

        private GameManager _gameManager;

        private List<CommandElementUI> _commands = new List<CommandElementUI>();

        #endregion

        #region Monobehaviour methods

        private void Start()
        {
            _gameManager = GameManager.Instance;

            PhotonNetwork.AddCallbackTarget(this);

            _gameManager.StateEnter += OnStateEnter;
            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
            _gameManager.CardHoverEnter += OnCardHoverEnter;
            _gameManager.CardHoverExit += OnCardHoverExit;
            _gameManager.CardSelected += OnCardSelected;
            _gameManager.CardCanceled += OnCardCanceled;
            _gameManager.CardPlaying += OnCardPlaying;

            exitButton.onClick.AddListener(OnExitButtonClicked);
            cancelExitButton.onClick.AddListener(OnCancelExitButtonClicked);
            confirmExitButton.onClick.AddListener(OnConfirmExitButtonClicked);

            highlightZoomImage.color = _gameManager.ColorSettings.CardPlayable;

            cardZoom.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            commandsContainer.gameObject.SetActive(false);
            exitModal.SetActive(false);
            header.SetActive(true);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            _gameManager.StateEnter -= OnStateEnter;
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
            _gameManager.CardHoverEnter -= OnCardHoverEnter;
            _gameManager.CardHoverExit -= OnCardHoverExit;
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
            _gameManager.CardPlaying -= OnCardPlaying;
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
                string currentPlayer = $"<b><color=#{_gameManager.ColorSettings.PlayerColor}>{PhotonNetwork.CurrentRoom.CurrentPlayer.NickName}</color></b>";
                if (state is TurnStartState)
                {
                    headerText.text = _gameManager.IsLocalPlayerTurn ? "Iniziando il tuo turno..."
                        : $"{currentPlayer} sta iniziando il turno...";
                }
                else if (state is DrawPhaseState)
                {
                    headerText.text = _gameManager.IsLocalPlayerTurn ? "Pescando..."
                        : $"{currentPlayer} sta pescando...";
                }
                else if (state is CardSelectionState)
                {
                    if (_gameManager.IsLocalPlayerTurn)
                    {
                        headerText.text = "Puoi giocare una carta o passare";

                        var passCommand = Instantiate(commandElementPrefab, commandsContainer);
                        passCommand.Configure("Passa", () =>
                        {
                            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.DiscardPhase });
                        });

                        _commands.ForEach(c => Destroy(c.gameObject));
                        _commands.Add(passCommand);

                        commandsContainer.gameObject.SetActive(true);
                    }
                    else
                    {
                        headerText.text = $"{currentPlayer} può giocare una carta o passare";

                        commandsContainer.gameObject.SetActive(false);
                    }
                }
                else if (state is CardResolutionState)
                {
                    string currentTarget = $"<b><color=#{_gameManager.ColorSettings.PlayerColor}>{PhotonNetwork.CurrentRoom.CurrentTarget.NickName}</color></b>";

                    var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                    if (card.Class == CardClass.Blue) return;

                    string missed = $"<color=#{_gameManager.ColorSettings.BrownCardColor}>Mancato</color>";
                    string bang = $"<color=#{_gameManager.ColorSettings.BrownCardColor}>Bang!</color>";
                    string damage = $"<color=#{_gameManager.ColorSettings.DamageColor}>danno</color>";

                    switch (card.Effect)
                    {
                        case CardEffect.Bang:
                        case CardEffect.Damage:
                        case CardEffect.Missed:
                            headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi giocare un {missed} o ricevere un {damage}"
                                : $"{currentTarget} deve giocare un {missed} o ricevere un {damage}";
                            break;
                        case CardEffect.Duel:
                        case CardEffect.Indians:
                            headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi giocare un {bang} o ricevere un {damage}"
                                : $"{currentTarget} deve giocare un {bang} o ricevere un {damage}";
                            break;
                        case CardEffect.Discard:
                            headerText.text = _gameManager.IsLocalPlayerTarget ? $"Puoi scegliere una carta da scartare"
                                : $"{currentTarget} può scegliere una carta da scartare";
                            break;
                        case CardEffect.GeneralStore:
                            headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi scegliere una carta"
                                : $"{currentTarget} deve scegliere una carta";
                            break;
                    }
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
            var instigator = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string message = card.Effect == CardEffect.Sceriff ? "{1} è lo {0}!" : "{1} era un {0}!";

            gameLog.Log(message, card, instigator);
        }

        private void OnCardPlaying(CardPlayingEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var instigator = PhotonNetwork.CurrentRoom.GetPlayer(eventData.InstigatorId);

            switch (card.Target)
            {
                default:
                case CardTarget.Self:
                case CardTarget.Everyone:
                case CardTarget.EveryoneElse:
                    gameLog.Log("{1} gioca {0}", card, instigator);
                    break;
                case CardTarget.Instigator:
                case CardTarget.Range:
                case CardTarget.FixedRange:
                case CardTarget.Anyone:
                    var target = PhotonNetwork.CurrentRoom.GetPlayer(eventData.TargetId);
                    gameLog.Log("{1} gioca {0} contro {2}", card, instigator, target);
                    break;
            }
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

        private void OnCardHoverEnter(CardHoverEnterEventData eventData)
        {
            highlightZoomImage.gameObject.SetActive(eventData.IsPlayable);
            cardZoomImage.sprite = _gameManager.CardSpriteTable.Get(eventData.CardView.CardData.Sprite);

            var screenPosition = Camera.main.WorldToScreenPoint(eventData.CardView.transform.position);
            bool bottom = screenPosition.y < Screen.height / 2;
            var anchor = new Vector2(0.5f, bottom ? 0 : 1);
            cardZoom.anchorMin = anchor;
            cardZoom.anchorMax = anchor;
            cardZoom.pivot = anchor;
            cardZoom.transform.position = screenPosition;

            cardZoom.gameObject.SetActive(true);
        }

        private void OnCardHoverExit()
        {
            cardZoomImage.sprite = null;

            cardZoom.gameObject.SetActive(false);
        }

        private void OnCardSelected(CardSelectedEventData eventData)
        {
            OnCardHoverExit();

            commandsContainer.gameObject.SetActive(false);
        }

        private void OnCardCanceled()
        {
            commandsContainer.gameObject.SetActive(false);
        }

        #endregion
    }
}