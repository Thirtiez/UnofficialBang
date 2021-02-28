using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class GameLogUI : MonoBehaviour
    {
        #region Inspector fields

        [Header("References")]

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private LogElementUI logElementPrefab;

        [Header("Colors")]

        [SerializeField]
        private Color brownCardColor;

        [SerializeField]
        private Color blueCardColor;

        [SerializeField]
        private Color characterCardColor;

        [SerializeField]
        private Color roleCardColor;

        [SerializeField]
        private Color targetColor;

        [SerializeField]
        private Color instigatorColor;

        #endregion

        #region Private fields

        private string hexBrownCardColor => ColorUtility.ToHtmlStringRGBA(brownCardColor);
        private string hexBlueCardColor => ColorUtility.ToHtmlStringRGBA(blueCardColor);
        private string hexCharacterCardColor => ColorUtility.ToHtmlStringRGBA(characterCardColor);
        private string hexRoleCardColor => ColorUtility.ToHtmlStringRGBA(characterCardColor);
        private string hexTargetColor => ColorUtility.ToHtmlStringRGBA(targetColor);
        private string hexInstigatorColor => ColorUtility.ToHtmlStringRGBA(instigatorColor);

        private List<string> _messages = new List<string>();

        private GameManager _gameManager;

        #endregion

        #region Monobehaviour callbacks

        protected void OnEnable()
        {
            _gameManager = GameManager.Instance;

            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
        }

        protected void OnDisable()
        {
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
        }

        #endregion

        #region Private methods

        private void Log(string message, CardData card = null, Player target = null, Player instigator = null)
        {
            string cardColor = null;

            if (card != null)
            {
                switch (card.Class)
                {
                    case CardClass.Brown:
                        cardColor = hexBrownCardColor;
                        break;

                    case CardClass.Blue:
                        cardColor = hexBlueCardColor;
                        break;

                    case CardClass.Character:
                        cardColor = hexCharacterCardColor;
                        break;

                    case CardClass.Role:
                        cardColor = hexRoleCardColor;
                        break;
                }
            }

            string cardName = $"<color=#{cardColor}>{card?.Name}</color>";
            string targetName = $"<color=#{hexTargetColor}>{target?.NickName}</color>";
            string instigatorName = $"<color=#{hexInstigatorColor}>{instigator?.NickName}</color>";

            message = string.Format(message, cardName, targetName, instigatorName);
            _messages.Add(message);

            var logElement = Instantiate(logElementPrefab, scrollRect.content);
            logElement.Configure(message);

            StartCoroutine(ScrollToBottom());
        }

        private IEnumerator ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            yield return null;

            scrollRect.verticalScrollbar.value = 0;
            Canvas.ForceUpdateCanvases();
        }

        #endregion

        #region Event handlers

        private void OnCardDealing(CardDealingEventData eventData)
        {
            if (eventData.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var card = _gameManager.Cards[eventData.CardId];

                switch (card.Class)
                {
                    case CardClass.Brown:
                    case CardClass.Blue:
                        Log("Hai pescato la carta {0}", card);
                        break;

                    case CardClass.Character:
                        Log("Hai pescato il personaggio {0}", card);
                        break;

                    case CardClass.Role:
                        Log("Hai pescato il ruolo {0}", card);
                        break;
                }
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);
            string message = card.IsSceriff ? "{1} è lo {0}!" : "{1} era un {0}!";

            Log(message, card, player);
        }

        #endregion
    }
}