using Photon.Pun;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class DiscardPhaseState : TurnState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void OnCardPickerExit(CardPickerExitEventData eventData)
        {
            var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                        int cardsToDiscardcount = PhotonNetwork.CurrentRoom.CurrentPlayer.DiscardCount;

            switch (card.Effect)
            {
                case CardEffect.Discard:
                    _gameManager.SendEvent(PhotonEvent.DiscardingCard, new DiscardingCardEventData
                    {
                        CardId = eventData.CardId,
                        PlayerId = PhotonNetwork.CurrentRoom.CurrentPlayerId,
                        TargetId = PhotonNetwork.CurrentRoom.CurrentTargetId,
                        IsFromHand = eventData.IsFromHand
                    });
                    break;

                case CardEffect.Panic:
                    _gameManager.SendEvent(PhotonEvent.StealingCard, new StealingCardEventData
                    {
                        CardId = eventData.CardId,
                        PlayerId = PhotonNetwork.CurrentRoom.CurrentPlayerId,
                        TargetId = PhotonNetwork.CurrentRoom.CurrentTargetId,
                        IsFromHand = eventData.IsFromHand
                    });
                    break;

                case CardEffect.GeneralStore:
                    PhotonNetwork.CurrentRoom.GeneralStoreCardIds = PhotonNetwork.CurrentRoom.GeneralStoreCardIds
                        .Where(c => c != eventData.CardId)
                        .ToArray();
                    _gameManager.SendEvent(PhotonEvent.DealingCard, new DealingCardEventData
                    {
                        CardId = eventData.CardId,
                        PlayerId = PhotonNetwork.CurrentRoom.CurrentTargetId,
                    });
                    break;
            }
        }
    }
}