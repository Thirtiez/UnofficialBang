using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class DiscardPhaseState : TurnState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.IsLocalPlayerTurn)
            {
                _gameManager.CardPickerEnter(new CardPickerEnterEventData { FaceUpCards = PhotonNetwork.CurrentRoom.CurrentPlayer.HandCardIds });

                _gameManager.CardPickerExit += OnCardPickerExit;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            _gameManager.CardPickerExit -= OnCardPickerExit;
        }

        private void OnCardPickerExit(CardPickerExitEventData eventData)
        {
            _gameManager.SendEvent(PhotonEvent.DiscardingCard, new DiscardingCardEventData
            {
                CardId = eventData.CardId,
                TargetId = PhotonNetwork.CurrentRoom.CurrentPlayerId,
                IsFromHand = true
            });

            if (PhotonNetwork.CurrentRoom.CurrentPlayer.DiscardCount > 1)
            {
                _gameManager.StartCoroutine(DelayCardPicker());
            }
            else
            {
                _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
            }

        }

        private IEnumerator DelayCardPicker()
        {
            yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);

            _gameManager.CardPickerEnter(new CardPickerEnterEventData { FaceUpCards = PhotonNetwork.CurrentRoom.CurrentPlayer.HandCardIds });
        }
    }
}