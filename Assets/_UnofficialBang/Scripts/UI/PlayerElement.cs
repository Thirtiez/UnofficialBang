using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class PlayerElement : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text readyText;

        [SerializeField]
        private CanvasGroup masterIcon;

        public Player Player { get; private set; }

        public void Configure(Player player)
        {
            Refresh(player);

            nameText.text = player.NickName;
        }

        public void Refresh(Player player)
        {
            Player = player;

            bool isReady = (bool)player.CustomProperties["ready"];
            readyText.text = isReady ? "PRONTO!" : "IN ATTESA...";

            masterIcon.alpha = player.IsMasterClient ? 1 : 0;
        }
    }
}