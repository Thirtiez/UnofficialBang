using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CharactersDealingState : PreparationState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (PhotonNetwork.IsMasterClient)
            {
                _gameManager.StartCoroutine(DealCharacters());
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

        private IEnumerator DealCharacters()
        {
            foreach (int playerId in PhotonNetwork.CurrentRoom.TurnPlayerIds)
            {
                var card = _gameManager.DrawCharacter();

                _gameManager.SendEvent(PhotonEvent.DealingCard, new DealingCardEventData { CardId = card.Id, PlayerId = playerId });

                yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);
            }

            yield return new WaitForSeconds(_gameManager.AnimationSettings.RoleRevealDelay);

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
        }
    }
}