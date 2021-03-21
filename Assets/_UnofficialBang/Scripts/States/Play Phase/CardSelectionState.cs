using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardSelectionState : PlayPhaseState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.IsLocalPlayerTurn)
            {
                var character = _gameManager.Cards[PhotonNetwork.LocalPlayer.CharacterCardId];
                switch (character.Effect)
                {
                    case CardEffect.SuzyLaFayette:
                        //TODO SuzyLaFayette
                        break;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    _gameManager.CardPlaying += OnCardPlaying;
                }
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _gameManager.CardPlaying -= OnCardPlaying;

            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void OnCardPlaying(CardPlayingEventData eventData)
        {
            PhotonNetwork.CurrentRoom.CurrentTargetId = eventData.TargetId;
            PhotonNetwork.CurrentRoom.CurrentCardId = eventData.CardId;

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.CardResolution });
        }
    }
}