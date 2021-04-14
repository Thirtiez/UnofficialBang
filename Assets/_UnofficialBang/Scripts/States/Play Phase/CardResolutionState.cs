using Photon.Pun;
using Sirenix.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardResolutionState : PlayPhaseState
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (_gameManager.IsLocalPlayerTarget)
            {
                var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                var targetPlayer = PhotonNetwork.CurrentRoom.CurrentTarget;

                bool isActionNeeded = false;
                switch (card.Effect)
                {
                    case CardEffect.Scope:
                        targetPlayer.Range += 1;
                        break;

                    case CardEffect.Mustang:
                        targetPlayer.BonusDistance += 1;
                        break;

                    case CardEffect.Weapon:
                        targetPlayer.BoardCardIds = targetPlayer.BoardCardIds.AppendWith(card.Id).ToArray();
                        if (card.Effect == CardEffect.Weapon)
                        {
                            targetPlayer.Range += card.EffectValue.Value;

                            var previousWeapon = targetPlayer.BoardCardIds?
                                .Select(c => _gameManager.Cards[c])?
                                .SingleOrDefault(c => c.Effect == CardEffect.Weapon || c.Effect == CardEffect.Volcanic);
                            if (previousWeapon != null)
                            {
                                _gameManager.SendEvent(PhotonEvent.DiscardingCard, new DiscardingCardEventData { CardId = previousWeapon.Id, PlayerId = targetPlayer.ActorNumber, IsFromHand = false });
                            }
                        }
                        break;

                    case CardEffect.Cure:
                    case CardEffect.Beer:
                        _gameManager.SendEvent(PhotonEvent.GainingHealth, new GainingHealthEventData { PlayerId = targetPlayer.ActorNumber, Amount = 1 });
                        break;

                    case CardEffect.Draw:
                        for (int i = 0; i < card.EffectValue.Value; i++)
                        {
                            _gameManager.SendEvent(PhotonEvent.DealingCard, new DealingCardEventData { CardId = card.Id, PlayerId = targetPlayer.ActorNumber });
                        }
                        break;

                    case CardEffect.GeneralStore:
                        if (PhotonNetwork.CurrentRoom.CurrentPlayerId == PhotonNetwork.CurrentRoom.CurrentTargetId)
                        {
                            //TODO General store
                        }
                        break;

                    default:
                        isActionNeeded = true;
                        break;
                }

                if (isActionNeeded)
                {
                    _gameManager.PlayingCard += OnPlayingCard;
                    _gameManager.TakingDamage += OnTakingDamage;
                }
                else
                {
                    GoForward();
                }
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _gameManager.PlayingCard -= OnPlayingCard;
            _gameManager.TakingDamage -= OnTakingDamage;

            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void GoForward()
        {
            int trigger = FSMTrigger.Forward;

            var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
            if (card.Target == CardTarget.Everyone || card.Target == CardTarget.EveryoneElse)
            {
                var nextTarget = _gameManager.NextLivingPlayerId;
                if (nextTarget != PhotonNetwork.CurrentRoom.CurrentPlayerId)
                {
                    trigger = FSMTrigger.CardResolution;
                    PhotonNetwork.CurrentRoom.CurrentTargetId = nextTarget;
                }
            }

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = trigger });
        }

        private void OnPlayingCard(PlayingCardEventData eventData)
        {
            GoForward();
        }

        private void OnTakingDamage(TakingDamageEventData eventData)
        {
            GoForward();
        }
    }
}