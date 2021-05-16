using Photon.Pun;
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

        private string _currentPlayerNickname => _gameManager.ColorSettings.Colorize(PhotonNetwork.CurrentRoom.CurrentPlayer.NickName, TextColorization.PlayerColor);
        private string _currentTargetNickname => _gameManager.ColorSettings.Colorize(PhotonNetwork.CurrentRoom.CurrentTarget.NickName, TextColorization.PlayerColor);
        private string _missedText => _gameManager.ColorSettings.Colorize("Mancato", TextColorization.BrownCard);
        private string _bangText => _gameManager.ColorSettings.Colorize("Bang!", TextColorization.BrownCard);
        private string _damageText => _gameManager.ColorSettings.Colorize("danno", TextColorization.DamageColor);
        private string _cureText => _gameManager.ColorSettings.Colorize("cura", TextColorization.CureColor);

        #endregion

        #region Monobehaviour methods

        private void Start()
        {
            _gameManager = GameManager.Instance;

            PhotonNetwork.AddCallbackTarget(this);

            _gameManager.StateEnter += OnStateEnter;
            _gameManager.DealingCard += OnDealingCard;
            _gameManager.RevealingRole += OnRevealingRole;
            _gameManager.CardHoverEnter += OnCardHoverEnter;
            _gameManager.CardHoverExit += OnCardHoverExit;
            _gameManager.CardSelected += OnCardSelected;
            _gameManager.CardCanceled += OnCardCanceled;
            _gameManager.PlayingCard += OnPlayingCard;
            _gameManager.TakingDamage += OnTakingDamage;
            _gameManager.GainingHealth += OnGainingHealth;
            _gameManager.DiscardingCard += OnDiscardingCard;

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
            _gameManager.DealingCard -= OnDealingCard;
            _gameManager.RevealingRole -= OnRevealingRole;
            _gameManager.CardHoverEnter -= OnCardHoverEnter;
            _gameManager.CardHoverExit -= OnCardHoverExit;
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
            _gameManager.PlayingCard -= OnPlayingCard;
            _gameManager.TakingDamage -= OnTakingDamage;
            _gameManager.GainingHealth -= OnGainingHealth;
            _gameManager.DiscardingCard -= OnDiscardingCard;
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
                if (state is TurnStartState)
                {
                    headerText.text = _gameManager.IsLocalPlayerTurn ? "Iniziando il tuo turno..."
                        : $"{_currentPlayerNickname} sta iniziando il turno...";
                }
                else if (state is DrawPhaseState)
                {
                    headerText.text = _gameManager.IsLocalPlayerTurn ? "Pescando..."
                        : $"{_currentPlayerNickname} sta pescando...";
                }
                else if (state is PlayPhaseState)
                {
                    if (state is CardSelectionState)
                    {
                        if (_gameManager.IsLocalPlayerTurn)
                        {
                            headerText.text = "Puoi giocare una carta o passare";

                            _commands.ForEach(c => Destroy(c.gameObject));
                            _commands.Clear();

                            var passCommand = Instantiate(commandElementPrefab, commandsContainer);
                            passCommand.Configure("Passa", () =>
                            {
                                _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.DiscardPhase });
                            });

                            _commands.Add(passCommand);

                            commandsContainer.gameObject.SetActive(true);
                        }
                        else
                        {
                            headerText.text = $"{_currentPlayerNickname} può giocare una carta o passare";

                            commandsContainer.gameObject.SetActive(false);
                        }
                    }
                    else if (state is CardResolutionState)
                    {
                        commandsContainer.gameObject.SetActive(false);

                        _commands.ForEach(c => Destroy(c.gameObject));
                        _commands.Clear();

                        var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                        if (card.Class == CardClass.Blue) return;

                        switch (card.Effect)
                        {
                            case CardEffect.Bang:
                            case CardEffect.Damage:
                            case CardEffect.Missed:
                                headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi giocare un {_missedText} o ricevere 1 {_damageText}"
                                    : $"{_currentTargetNickname} deve giocare un {_missedText} o ricevere 1 {_damageText}";

                                if (_gameManager.IsLocalPlayerTarget)
                                {
                                    commandsContainer.gameObject.SetActive(true);

                                    var damageCommand = Instantiate(commandElementPrefab, commandsContainer);
                                    damageCommand.Configure("Ricevi 1 danno", () =>
                                        _gameManager.SendEvent(PhotonEvent.TakingDamage, new TakingDamageEventData { PlayerId = PhotonNetwork.LocalPlayer.ActorNumber, Amount = 1 }));
                                    _commands.Add(damageCommand);
                                }
                                break;

                            case CardEffect.Duel:
                            case CardEffect.Indians:
                                headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi giocare un {_bangText} o ricevere 1 {_damageText}"
                                    : $"{_currentTargetNickname} deve giocare un {_bangText} o ricevere 1 {_damageText}";

                                if (_gameManager.IsLocalPlayerTarget)
                                {
                                    commandsContainer.gameObject.SetActive(true);

                                    var damageCommand = Instantiate(commandElementPrefab, commandsContainer);
                                    damageCommand.Configure("Ricevi 1 danno", () =>
                                        _gameManager.SendEvent(PhotonEvent.TakingDamage, new TakingDamageEventData { PlayerId = PhotonNetwork.LocalPlayer.ActorNumber, Amount = 1 }));
                                    _commands.Add(damageCommand);
                                }
                                break;

                            case CardEffect.Discard:
                                headerText.text = _gameManager.IsLocalPlayerTarget ? $"Puoi scegliere una carta da scartare"
                                    : $"{_currentTargetNickname} può scegliere una carta da scartare";
                                break;

                            case CardEffect.GeneralStore:
                                headerText.text = _gameManager.IsLocalPlayerTarget ? $"Devi scegliere una carta"
                                    : $"{_currentTargetNickname} deve scegliere una carta";
                                break;
                        }
                    }
                }
            }
        }

        private void OnDealingCard(DealingCardEventData eventData)
        {
            if (eventData.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var card = _gameManager.Cards[eventData.CardId];

                switch (card.Class)
                {
                    case CardClass.Brown:
                        gameLog.Log($"Hai pescato una carta {_gameManager.ColorSettings.Colorize(card.Name, TextColorization.BrownCard)}");
                        break;

                    case CardClass.Blue:
                        gameLog.Log($"Hai pescato una carta {_gameManager.ColorSettings.Colorize(card.Name, TextColorization.BlueCard)}");
                        break;

                    case CardClass.Character:
                        gameLog.Log($"Hai pescato il personaggio {_gameManager.ColorSettings.Colorize(card.Name, TextColorization.CharacterCard)}");
                        break;

                    case CardClass.Role:
                        gameLog.Log($"Hai pescato il ruolo {_gameManager.ColorSettings.Colorize(card.Name, TextColorization.RoleCard)}");
                        break;
                }
            }
        }

        private void OnRevealingRole(RevealingRoleEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);

            string characterName = _gameManager.ColorSettings.Colorize(card.Name, TextColorization.CharacterCard);
            string playerName = _gameManager.ColorSettings.Colorize(player.NickName, TextColorization.PlayerColor);

            gameLog.Log(card.Effect == CardEffect.Sceriff ? $"{playerName} è lo {characterName}!" : $"{playerName} era un {characterName}!");
        }

        private void OnPlayingCard(PlayingCardEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var instigator = PhotonNetwork.CurrentRoom.GetPlayer(eventData.InstigatorId);

            var cardColorization = card.Class == CardClass.Blue ? TextColorization.BlueCard : TextColorization.BrownCard;
            string cardName = _gameManager.ColorSettings.Colorize(card.Name, cardColorization);
            string instigatorName = _gameManager.ColorSettings.Colorize(instigator.NickName, TextColorization.PlayerColor);

            switch (card.Target)
            {
                default:
                case CardTarget.Self:
                case CardTarget.Everyone:
                case CardTarget.EveryoneElse:
                    gameLog.Log($"{instigatorName} gioca {cardName}");
                    break;
                case CardTarget.Instigator:
                case CardTarget.Range:
                case CardTarget.FixedRange:
                case CardTarget.Anyone:
                    var target = PhotonNetwork.CurrentRoom.GetPlayer(eventData.TargetId);
                    string targetName = _gameManager.ColorSettings.Colorize(target.NickName, TextColorization.PlayerColor);
                    gameLog.Log($"{instigatorName} gioca {cardName} contro {targetName}");
                    break;
            }
        }

        private void OnTakingDamage(TakingDamageEventData eventData)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string playerName = _gameManager.ColorSettings.Colorize(player.NickName, TextColorization.PlayerColor);

            gameLog.Log($"{playerName} riceve {eventData.Amount} {_damageText}");
        }

        private void OnGainingHealth(GainingHealthEventData eventData)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string playerName = _gameManager.ColorSettings.Colorize(player.NickName, TextColorization.PlayerColor);

            gameLog.Log($"{playerName} riceve {eventData.Amount} {_cureText}");
        }

        private void OnDiscardingCard(DiscardingCardEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string playerName = _gameManager.ColorSettings.Colorize(player.NickName, TextColorization.PlayerColor);

            var cardColorization = card.Class == CardClass.Blue ? TextColorization.BlueCard : TextColorization.BrownCard;
            string cardName = _gameManager.ColorSettings.Colorize(card.Name, cardColorization);

            gameLog.Log($"{playerName} scarta una carta {cardName}");
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

        private void OnCardSelected(SelectingCardEventData eventData)
        {
            OnCardHoverExit();

            commandsContainer.gameObject.SetActive(false);
        }

        private void OnCardCanceled()
        {
            commandsContainer.gameObject.SetActive(true);
        }

        #endregion
    }
}