using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardsDealingState : PreparationState
    {
        [SerializeField]
        private float dealCardDelay = 0.2f;

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

                foreach (Player player in _gameManager.Players)
                {
                    var playerProperties = _gameManager.GetPlayerProperties(player);
                    if (playerProperties.HandCount < playerProperties.MaxHealth)
                    {
                        var card = _gameManager.DrawPlayingCard();

                        _gameManager.SendEvent(PhotonEvent.CardDealing, new CardDealingEventData { CardId = card.Id, PlayerId = player.ActorNumber });
                        yield return new WaitForSeconds(dealCardDelay);

                        keepDealing = true;
                    }
                }
            }

            _gameManager.CurrentPlayer = _gameManager.Players[0];

            GoTo(FSMTrigger.Forward);
        }
    }
}