using Photon.Pun;
using Photon.Realtime;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Thirties.UnofficialBang
{
    public class CardResolutionState : PlayPhaseState
    {
        private int _cardsToDraw = 0;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (PhotonNetwork.IsMasterClient)
            {
                var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                var targetPlayer = PhotonNetwork.CurrentRoom.CurrentTarget;

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
                        _gameManager.StartCoroutine(DrawCards(targetPlayer, card.EffectValue.Value));
                        break;

                    case CardEffect.GeneralStore:
                        if (PhotonNetwork.CurrentRoom.CurrentPlayerId == PhotonNetwork.CurrentRoom.CurrentTargetId)
                        {
                            //TODO General store
                        }
                        break;
                }
            }
            
            if (_gameManager.IsLocalPlayerTarget)
            {
                var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];

                switch (card.Effect)
                {
                    case CardEffect.Bang:
                    case CardEffect.Damage:
                    case CardEffect.Duel:
                    case CardEffect.Indians:
                        _gameManager.PlayingCard += OnPlayingCard;
                        _gameManager.TakingDamage += OnTakingDamage;
                        break;

                    case CardEffect.Draw:
                        _cardsToDraw = card.EffectValue.Value;
                        _gameManager.DealingCard += OnDealingCard;
                        break;

                    case CardEffect.Beer:
                    case CardEffect.Cure:
                        _gameManager.GainingHealth += OnGainingHealth;
                        break;

                    case CardEffect.Discard:
                    case CardEffect.Panic:
                    case CardEffect.GeneralStore:
                        break;

                    default:
                        GoForward();
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
            _gameManager.PlayingCard -= OnPlayingCard;
            _gameManager.TakingDamage -= OnTakingDamage;
            _gameManager.DealingCard -= OnDealingCard;
            _gameManager.GainingHealth -= OnGainingHealth;

            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private void GoForward(bool tookDamage = false)
        {
            int trigger = FSMTrigger.Forward;

            var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
            if (card.Target == CardTarget.Everyone || card.Target == CardTarget.EveryoneElse)
            {
                var nextTarget = _gameManager.GetNextLivingPlayerId(PhotonNetwork.LocalPlayer.ActorNumber);
                if (nextTarget != PhotonNetwork.CurrentRoom.CurrentPlayerId)
                {
                    trigger = FSMTrigger.CardResolution;
                    PhotonNetwork.CurrentRoom.CurrentTargetId = nextTarget;
                }
            }
            else if (!tookDamage && card.Effect == CardEffect.Duel)
            {
                int currentTargetId = PhotonNetwork.LocalPlayer.ActorNumber;
                PhotonNetwork.CurrentRoom.CurrentTargetId = PhotonNetwork.CurrentRoom.LastDuelTargetId;
                PhotonNetwork.CurrentRoom.LastDuelTargetId = currentTargetId;

                trigger = FSMTrigger.CardResolution;
            }

            _gameManager.SendEvent(PhotonEvent.ChangingState, new ChangingStateEventData { Trigger = trigger });
        }

        private void OnPlayingCard(PlayingCardEventData eventData)
        {
            GoForward();
        }

        private void OnTakingDamage(TakingDamageEventData eventData)
        {
            GoForward(true);
        }

        private void OnDealingCard(DealingCardEventData eventData)
        {
            _cardsToDraw--;
            if (_cardsToDraw <= 0)
            {
                GoForward();
            }
        }

        private void OnGainingHealth(GainingHealthEventData eventData)
        {
            _gameManager.StartCoroutine(DelayAction(_gameManager.AnimationSettings.BulletAnimationDelay, () => GoForward()));
        }

        private IEnumerator DrawCards(Player targetPlayer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var card = _gameManager.DrawPlayingCard();
                _gameManager.SendEvent(PhotonEvent.DealingCard, new DealingCardEventData { CardId = card.Id, PlayerId = targetPlayer.ActorNumber });

                yield return new WaitForSeconds(_gameManager.AnimationSettings.DealCardDelay);
            }
        }

        private IEnumerator DelayAction(float amount, UnityAction action)
        {
            yield return new WaitForSeconds(amount);

            action.Invoke();
        }
    }
}