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

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private LogElementUI logElementPrefab;

        #endregion

        #region Private fields

        private List<string> _messages = new List<string>();

        #endregion

        #region Private methods

        private IEnumerator ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            yield return null;

            scrollRect.verticalScrollbar.value = 0;
            Canvas.ForceUpdateCanvases();
        }

        #endregion

        #region Public methods

        public void Log(string message, CardData card = null, Player target = null, Player instigator = null)
        {
            var colorSettings = GameManager.Instance.ColorSettings;
            string cardColor = null;

            if (card != null)
            {
                switch (card.Class)
                {
                    case CardClass.Brown:
                        cardColor = colorSettings.BrownCardColor;
                        break;

                    case CardClass.Blue:
                        cardColor = colorSettings.BlueCardColor;
                        break;

                    case CardClass.Character:
                        cardColor = colorSettings.CharacterCardColor;
                        break;

                    case CardClass.Role:
                        cardColor = colorSettings.RoleCardColor;
                        break;
                }
            }

            string cardName = $"<color=#{cardColor}>{card?.Name}</color>";
            string targetName = $"<color=#{colorSettings.PlayerColor}>{target?.NickName}</color>";
            string instigatorName = $"<color=#{colorSettings.PlayerColor}>{instigator?.NickName}</color>";

            message = string.Format(message, cardName, targetName, instigatorName);
            _messages.Add(message);

            var logElement = Instantiate(logElementPrefab, scrollRect.content);
            logElement.Configure(message);

            StartCoroutine(ScrollToBottom());
        }

        #endregion
    }
}