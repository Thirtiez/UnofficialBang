using Photon.Pun;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class InitializationState : BaseState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _gameManager.InitializePlayer();

            if (PhotonNetwork.IsMasterClient)
            {
                _gameManager.InitializeDecks();

                GoTo(FSMTrigger.Forward);
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
    }
}