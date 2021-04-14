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

        public void Log(string message)
        {
            _messages.Add(message);

            var logElement = Instantiate(logElementPrefab, scrollRect.content);
            logElement.Configure(message);

            StartCoroutine(ScrollToBottom());
        }

        #endregion
    }
}