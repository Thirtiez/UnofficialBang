using Photon.Realtime;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class GameLog : MonoBehaviour, IRecyclableScrollRectDataSource
    {
        [Header("References")]

        [SerializeField]
        private RecyclableScrollRect recyclableScrollRect;
        [SerializeField]
        private Scrollbar scrollbar;

        [Header("Colors")]

        [SerializeField]
        private Color cardColor = Color.red;
        [SerializeField]
        private Color targetColor = Color.yellow;
        [SerializeField]
        private Color instigatorColor = Color.yellow;

        private string hexCardColor => ColorUtility.ToHtmlStringRGBA(cardColor);
        private string hexTargetColor => ColorUtility.ToHtmlStringRGBA(targetColor);
        private string hexInstigatorColor => ColorUtility.ToHtmlStringRGBA(instigatorColor);

        private List<string> messages = new List<string>();

        protected void Awake()
        {
            recyclableScrollRect.DataSource = this;
        }

        public void Log(string message, CardData card = null, Player target = null, Player instigator = null)
        {
            string cardName = $"<color=#{hexCardColor}>{card?.Name}</color>";
            string targetName = $"<color=#{hexTargetColor}>{target?.NickName}</color>";
            string instigatorName = $"<color=#{hexInstigatorColor}>{instigator?.NickName}</color>";

            message = string.Format(message, cardName, targetName, instigatorName);
            messages.Add(message);

            recyclableScrollRect.ReloadData();
        }

        #region Data source

        public int GetItemCount()
        {
            return messages.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var logElement = cell as LogElement;
            logElement.Configure(messages[index]);
        }

        #endregion
    }
}