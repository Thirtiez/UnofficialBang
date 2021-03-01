﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class RolesDealingState : PreparationState
    {
        [SerializeField]
        private float dealCardDelay = 0.2f;

        [SerializeField]
        private float sceriffRevealDelay = 1.0f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.Instance.StartCoroutine(DealRoles());
            }

            GameManager.Instance.RoleRevealing += OnRoleRevealing;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            GameManager.Instance.RoleRevealing -= OnRoleRevealing;

            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private IEnumerator DealRoles()
        {
            RoleRevealingEventData revealRoleEventData = null;

            foreach (Player player in _gameManager.Players)
            {
                var card = _gameManager.DrawRole();

                _gameManager.SendEvent(PhotonEvent.CardDealing, new CardDealingEventData { CardId = card.Id, PlayerId = player.ActorNumber });

                if (card.IsSceriff)
                {
                    revealRoleEventData = new RoleRevealingEventData { CardId = card.Id, PlayerId = player.ActorNumber };
                }

                yield return new WaitForSeconds(dealCardDelay);
            }

            if (revealRoleEventData != null)
            {
                _gameManager.SendEvent(PhotonEvent.RoleRevealing, revealRoleEventData);

                yield return new WaitForSeconds(sceriffRevealDelay);
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            var card = _gameManager.Cards[eventData.CardId];
            if (card.Class == CardClass.Role)
            {
                GoTo(FSMTrigger.Forward);
            }
        }
    }
}