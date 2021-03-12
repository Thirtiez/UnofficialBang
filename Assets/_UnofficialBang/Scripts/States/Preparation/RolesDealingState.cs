using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class RolesDealingState : PreparationState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (PhotonNetwork.IsMasterClient)
            {
                GameManager.Instance.StartCoroutine(DealRoles());
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private IEnumerator DealRoles()
        {
            RoleRevealingEventData revealRoleEventData = null;

            foreach (int playerId in PhotonNetwork.CurrentRoom.TurnPlayerIds)
            {
                var card = _gameManager.DrawRole();

                _gameManager.SendEvent(PhotonEvent.CardDealing, new CardDealingEventData { CardId = card.Id, PlayerId = playerId });

                if (card.Effect == CardEffect.Sceriff)
                {
                    revealRoleEventData = new RoleRevealingEventData { CardId = card.Id, PlayerId = playerId };
                }

                yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);
            }

            if (revealRoleEventData != null)
            {
                _gameManager.SendEvent(PhotonEvent.RoleRevealing, revealRoleEventData);

                yield return new WaitForSeconds(_gameManager.AnimationSettings.RoleRevealDelay);
            }

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
        }
    }
}