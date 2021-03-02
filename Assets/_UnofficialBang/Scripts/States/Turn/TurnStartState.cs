using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class TurnStartState : TurnState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.CurrentPlayer == PhotonNetwork.LocalPlayer)
            {
                if (_gameManager.PlayerBoard.Any(c => c.Effect == CardEffect.Dynamite))
                {
                    //TODO Dynamite
                }
                else if (_gameManager.PlayerBoard.Any(c => c.Effect == CardEffect.Prison))
                {
                    //TODO Prison
                }
                else
                {
                    _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
                }
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