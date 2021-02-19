﻿using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class BaseState : StateMachineBehaviour
    {
        protected GameManager _gameManager;

        private Animator _fsm;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _fsm = animator;

            if (_gameManager == null && GameManager.Instance != null)
            {
                _gameManager = GameManager.Instance;
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

        protected void GoTo(int trigger)
        {
            _fsm.SetTrigger(trigger);
        }
    }
}