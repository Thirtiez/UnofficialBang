using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class BaseState : StateMachineBehaviour
    {
        protected GameManager _gameManager;

        private Animator _fsm;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            Debug.Log($"<color=green> Entered {GetType().Name}</color>");

            _fsm = animator;

            _gameManager = GameManager.Instance;

            _gameManager.CurrentState = this;
            _gameManager.StateEnter?.Invoke(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            _gameManager.StateExit?.Invoke(this);

            Debug.Log($"<color=green> Exited {GetType().Name}</color>");
        }
    }
}