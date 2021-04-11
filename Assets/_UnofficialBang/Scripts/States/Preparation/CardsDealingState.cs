using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardsDealingState : PreparationState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (PhotonNetwork.IsMasterClient)
            {
                _gameManager.StartCoroutine(DealCards());
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

        private IEnumerator DealCards()
        {
            bool keepDealing = true;
            while (keepDealing)
            {
                keepDealing = false;

                foreach (int playerId in PhotonNetwork.CurrentRoom.TurnPlayerIds)
                {
                    var player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);
                    if (player.HandCardIds.Length < player.MaxHealth)
                    {
                        var card = _gameManager.DrawPlayingCard();

                        _gameManager.SendEvent(PhotonEvent.DealingCard, new DealingCardEventData { CardId = card.Id, PlayerId = playerId });
                        yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);

                        keepDealing = true;
                    }
                }
            }

            PhotonNetwork.CurrentRoom.CurrentPlayerId = PhotonNetwork.CurrentRoom.TurnPlayerIds[0];

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
        }
    }
}