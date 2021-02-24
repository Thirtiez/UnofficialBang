using Photon.Pun;
using Photon.Realtime;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirties.UnofficialBang
{
    public class GameLogUI : MonoBehaviour, IRecyclableScrollRectDataSource
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

        private List<string> _messages = new List<string>();

        private GameManager _gameManager;

        #region Monobehaviour callbacks

        protected void Awake()
        {
            recyclableScrollRect.DataSource = this;
        }

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
            string cardName = $"<color=#{hexCardColor}>{card?.Name}</color>";
            string targetName = $"<color=#{hexTargetColor}>{target?.NickName}</color>";
            string instigatorName = $"<color=#{hexInstigatorColor}>{instigator?.NickName}</color>";

            message = string.Format(message, cardName, targetName, instigatorName);
            _messages.Add(message);

            recyclableScrollRect.ReloadData();
        }

        #endregion

        #region Data source

        public int GetItemCount()
        {
            return _messages.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var logElement = cell as LogElementUI;
            logElement.Configure(_messages[index]);
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
            string message = card.IsSceriff ? "{1} è lo {0}" : "{1} era un {0}";

            Log(message, card, player);
        }

        #endregion
    }
}