using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class DrawPhaseState : TurnState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.IsLocalPlayerTurn)
            {
                var character = _gameManager.Cards[PhotonNetwork.LocalPlayer.CharacterCardId];
                switch (character.Effect)
                {
                    //case CardEffect.BlackJack:
                    //    //TODO BlackJack
                    //    break;
                    //case CardEffect.JesseJones:
                    //    //TODO JesseJones
                    //    break;
                    //case CardEffect.KitCarlson:
                    //    //TODO KitCarlson
                    //    break;
                    //case CardEffect.PedroRamirez:
                    //    //TODO PedroRamirez
                    //    break;
                    default:
                        _gameManager.StartCoroutine(DrawCards());
                        break;
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

        private IEnumerator DrawCards()
        {
            for (int i = 0; i < 2; i++)
            {
                var card = _gameManager.DrawPlayingCard();
                _gameManager.SendEvent(PhotonEvent.CardDealing, new CardDealingEventData { CardId = card.Id, PlayerId = PhotonNetwork.LocalPlayer.ActorNumber });

                yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);
            }

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = FSMTrigger.Forward });
        }
    }
}