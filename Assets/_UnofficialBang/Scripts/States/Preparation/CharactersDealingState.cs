using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CharactersDealingState : PreparationState
    {
        [SerializeField]
        private float dealCardDelay = 0.2f;

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
            foreach (Player player in _gameManager.Players)
            {
                var card = _gameManager.DrawCharacter();

                _gameManager.SendEvent(PhotonEvent.CardDealing, new CardDealingEventData { CardId = card.Id, PlayerId = player.ActorNumber });

                yield return new WaitForSeconds(dealCardDelay);
            }

            GoTo(FSMTrigger.Forward);
        }
    }
}