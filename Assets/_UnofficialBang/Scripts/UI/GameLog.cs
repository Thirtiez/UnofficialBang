using ExitGames.Client.Photon;
using OneP.InfinityScrollView;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public enum LogTemplate
    {
        RoleRevealed,
        CharacterRevealed,
        GameStarted
    }

    public class GameLog : MonoBehaviour, IOnEventCallback
    {
        [SerializeField]
        private InfinityScrollView infinityScrollView;

        [OdinSerialize]
        private Dictionary<LogTemplate, string> templates;

        public List<string> Messages { get; private set; } = new List<string>();

        protected void Awake()
        {
            infinityScrollView.Setup(Messages.Count);
        }

        protected void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        protected void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Log(LogTemplate template, CardData card = null, Player instigator = null, Player target = null)
        {
            string message = string.Format(templates[template], card?.Name, target?.NickName, instigator?.NickName);
            Messages.Add(message);

            infinityScrollView.Setup(Messages.Count);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case GameEvent.RevealRole:
                    var eventData = photonEvent.CustomData as RevealRoleEventData;
                    Log(LogTemplate.RoleRevealed, eventData.Card, eventData.Player);
                    break;
            }
        }
    }
}