using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Utilities;
using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class PlayerView : MonoBehaviour
    {
        #region Static fields

        private static List<PlayerView> _playerViews = new List<PlayerView>();

        #endregion

        #region Inspector fields

        [Header("Player")]

        [SerializeField]
        private int playerNumber;

        [SerializeField]
        private TMP_Text nicknameText;

        [SerializeField]
        private List<GameObject> bullets;

        [Header("Splines")]

        [SerializeField]
        private Spline sideSpline;

        [SerializeField]
        private Spline handSpline;

        [SerializeField]
        private Spline boardSpline;

        [Header("Cards")]

        [SerializeField]
        [Range(0, 1)]
        private float cardPreferredDistance = 0.2f;

        [SerializeField]
        private CardView cardPrefab;

        [Header("Deck")]

        [SerializeField]
        private Transform deckTransform;

        [SerializeField]
        private Transform discardTransform;

        #endregion

        #region Public properties

        public int PlayerId { get; private set; }
        public int PlayerDistance { get; private set; }
        public bool IsLocalPlayer => playerNumber == 0;
        public bool IsCurrentPlayer => PlayerId == PhotonNetwork.CurrentRoom.CurrentPlayerId;
        public bool IsCurrentTarget => PlayerId == PhotonNetwork.CurrentRoom.CurrentTargetId;
        public bool IsCurrentPlayerOrTarget =>
            (_gameManager.CurrentState is CardSelectionState && IsCurrentPlayer) ||
            (_gameManager.CurrentState is CardResolutionState && IsCurrentTarget);

        #endregion

        #region Private fields

        private GameManager _gameManager;

        private List<CardView> _sideCards = new List<CardView>();
        private List<CardView> _handCards = new List<CardView>();
        private List<CardView> _boardCards = new List<CardView>();
        private List<CardView> _discardedCards = new List<CardView>();

        private CardView _roleCard;
        private CardView _characterCard;

        private Player _player => PhotonNetwork.CurrentRoom.GetPlayer(PlayerId);

        #endregion

        #region Constants

        private readonly Dictionary<int, List<int>> EnableTable = new Dictionary<int, List<int>>
        {
            { 1, new List<int> {0} },
            { 2, new List<int> {0, 4} },
            { 3, new List<int> {0, 3, 5} },
            { 4, new List<int> {0, 2, 4, 6} },
            { 5, new List<int> {0, 1, 3, 5, 7} },
            { 6, new List<int> {0, 1, 3, 4, 5, 7} },
            { 7, new List<int> {0, 1, 2, 3, 5, 6, 7} },
            { 8, new List<int> {0, 1, 2, 3, 4, 5, 6, 7} },
        };

        #endregion

        #region Monobehaviour callbacks

        protected void OnEnable()
        {
            _playerViews.Add(this);

            _gameManager = GameManager.Instance;

            _gameManager.StateEnter += OnStateEnter;
            _gameManager.StateExit += OnStateExit;
            _gameManager.DealingCard += OnDealingCard;
            _gameManager.RevealingRole += OnRevealingRole;
            _gameManager.PlayingCard += OnPlayingCard;
            _gameManager.TakingDamage += OnTakingDamage;
            _gameManager.GainingHealth += OnGainingHealth;
            _gameManager.DiscardingCard += OnDiscardingCard;
            _gameManager.StealingCard += OnStealingCard;
        }

        protected void OnDisable()
        {
            _playerViews.Remove(this);

            _gameManager.StateEnter -= OnStateEnter;
            _gameManager.StateExit -= OnStateExit;
            _gameManager.DealingCard -= OnDealingCard;
            _gameManager.RevealingRole -= OnRevealingRole;
            _gameManager.PlayingCard -= OnPlayingCard;
            _gameManager.TakingDamage -= OnTakingDamage;
            _gameManager.GainingHealth -= OnGainingHealth;
            _gameManager.DiscardingCard -= OnDiscardingCard;
            _gameManager.StealingCard -= OnStealingCard;
        }

        #endregion

        #region Private methods

        private void Configure()
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            bool isActive = EnableTable[playerCount].Contains(playerNumber);
            gameObject.SetActive(isActive);

            if (isActive)
            {
                int localPlayerIndex = Array.IndexOf(PhotonNetwork.CurrentRoom.TurnPlayerIds, PhotonNetwork.LocalPlayer.ActorNumber);
                int playerOffset = EnableTable[playerCount].IndexOf(playerNumber);
                int playerIndex = (localPlayerIndex + playerOffset) % playerCount;

                PlayerDistance = playerOffset > playerCount / 2 ? playerCount - playerOffset : playerOffset;
                PlayerId = PhotonNetwork.CurrentRoom.TurnPlayerIds[playerIndex];
                nicknameText.text = _player.NickName;

                bullets.ForEach(b =>
                {
                    b.gameObject.SetActive(false);
                    b.transform.localScale = Vector3.zero;
                });
            }
        }

        private void DealCard(CardData cardData, Transform fromTransform = null, Spline toSpline = null, List<CardView> cardList = null, bool isCovered = false)
        {
            fromTransform = fromTransform ?? deckTransform;
            toSpline = toSpline ?? handSpline;
            cardList = cardList ?? _handCards;

            var card = Instantiate(cardPrefab, fromTransform.position, fromTransform.rotation, toSpline.transform);
            card.Configure(cardData, isCovered);

            cardList.Add(card);

            RefreshSpline(toSpline, cardList);
        }

        private void EquipCard(CardView card)
        {
            _boardCards.Add(card);
            card.transform.SetParent(boardSpline.transform);
            RefreshSpline(boardSpline, _boardCards);
        }

        private void DiscardCard(CardView card)
        {
            _discardedCards.FirstOrDefault()?.MoveTo(new Vector3(0, 0, 0.1f), Quaternion.identity, Vector3.one);

            _discardedCards.Insert(0, card);
            _discardedCards.Skip(2)?.ForEach(c => Destroy(c.gameObject));
            _discardedCards = _discardedCards.Take(2).ToList();

            card.transform.SetParent(discardTransform);
            card.MoveTo(Vector3.zero, Quaternion.identity, Vector3.one);
        }

        private void RefreshSpline(Spline targetSpline, List<CardView> cardList)
        {
            float possibleDistance = 1f / cardList.Count;
            float distance = possibleDistance >= cardPreferredDistance ? cardPreferredDistance : possibleDistance;
            float startTime = (1 - (distance * (cardList.Count - 1))) * 0.5f;

            for (int i = 0; i < cardList.Count; i++)
            {
                var curve = targetSpline.GetSample(startTime + distance * i);
                var position = curve.location + new Vector3(0, 0, -0.01f * i);
                var rotation = Quaternion.LookRotation(Vector3.forward, curve.up);

                cardList[i].MoveTo(position, rotation, Vector3.one);
            }
        }

        private IEnumerator GainBulletRoutine(int from, int to)
        {
            for (int i = from; i < to; i++)
            {
                GainBullet(i);
                yield return new WaitForSeconds(_gameManager.AnimationSettings.BulletAnimationDelay);
            }
        }

        private void GainBullet(int index)
        {
            var bullet = bullets[index];
            bullet.gameObject.SetActive(true);
            bullet.transform
                .DOScale(Vector3.one * 0.1f, _gameManager.AnimationSettings.BulletAnimationDuration)
                .SetEase(Ease.OutBack);
        }

        private IEnumerator LoseBulletRoutine(int from, int to)
        {
            for (int i = from; i >= to; i--)
            {
                LoseBullet(i);
                yield return new WaitForSeconds(_gameManager.AnimationSettings.BulletAnimationDelay);
            }
        }

        private void LoseBullet(int index)
        {
            var bullet = bullets[index];
            bullet.transform
                .DOScale(Vector3.zero, _gameManager.AnimationSettings.BulletAnimationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => bullet.gameObject.SetActive(false));
        }

        private void ConfigurePlayableCards(BaseState state)
        {
            if (_gameManager.IsLocalPlayerTurn && state is CardSelectionState)
            {
                var excludedEffects = new List<CardEffect>();

                var equipments = PhotonNetwork.LocalPlayer.BoardCardIds
                    .Select(c => _gameManager.Cards[c])
                    .Where(c => c.Effect == CardEffect.Barrel
                        || c.Effect == CardEffect.Scope
                        || c.Effect == CardEffect.Mustang)
                    .ToList();
                if (equipments.Any())
                {
                    excludedEffects.AddRange(equipments.Select(c => c.Effect));
                }

                if (_characterCard.CardData.Effect != CardEffect.CalamityJanet)
                {
                    excludedEffects.Add(CardEffect.Missed);
                }

                _handCards.ForEach(c => c.SetPlayable(!excludedEffects.Contains(c.CardData.Effect)));
            }
            else if (_gameManager.IsLocalPlayerTarget && state is CardResolutionState)
            {
                var card = _gameManager.Cards[PhotonNetwork.CurrentRoom.CurrentCardId];
                switch (card.Effect)
                {
                    case CardEffect.Bang:
                    case CardEffect.Damage:
                    case CardEffect.Missed:
                        _handCards.ForEach(c => c.SetPlayable(
                            c.CardData.Effect == CardEffect.Missed ||
                            (c.CardData.Effect == CardEffect.Bang && _characterCard.CardData.Effect == CardEffect.CalamityJanet)));
                        break;

                    case CardEffect.Duel:
                    case CardEffect.Indians:
                        _handCards.ForEach(c => c.SetPlayable(
                            c.CardData.Effect == CardEffect.Bang ||
                            (c.CardData.Effect == CardEffect.Missed && _characterCard.CardData.Effect == CardEffect.CalamityJanet)));
                        break;
                }
            }
            else
            {
                _handCards.ForEach(c => c.SetPlayable(false));

                _characterCard.SetPlayable(false);
            }
        }

        #endregion

        #region Event handlers

        private void OnStateEnter(BaseState state)
        {
            if (state is RolesDealingState)
            {
                Configure();
            }
            else if (IsLocalPlayer && state is PlayPhaseState)
            {
                ConfigurePlayableCards(state);
            }
        }

        private void OnStateExit(BaseState state)
        {
            if (state is CharactersDealingState)
            {
                StartCoroutine(GainBulletRoutine(0, _player.CurrentHealth));
            }
        }

        private void OnDealingCard(DealingCardEventData eventData)
        {
            if (eventData.PlayerId == PlayerId)
            {
                var cardData = _gameManager.Cards[eventData.CardId];

                if (cardData.Class == CardClass.Blue || cardData.Class == CardClass.Brown)
                {
                    bool isCovered = eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber;
                    DealCard(cardData, deckTransform, handSpline, _handCards, isCovered);
                }
                else if (cardData.Class == CardClass.Character)
                {
                    DealCard(cardData, deckTransform, sideSpline, _sideCards);

                    _characterCard = _sideCards.Last();
                }
                else if (cardData.Class == CardClass.Role)
                {
                    bool isCovered = eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber;
                    DealCard(cardData, deckTransform, sideSpline, _sideCards, isCovered);

                    _roleCard = _sideCards.Last();
                }
            }
        }

        private void OnRevealingRole(RevealingRoleEventData eventData)
        {
            if (!IsLocalPlayer && eventData.PlayerId == PlayerId)
            {
                _roleCard.Show();
            }
        }

        private void OnPlayingCard(PlayingCardEventData eventData)
        {
            if (IsCurrentPlayerOrTarget)
            {
                var card = _handCards.SingleOrDefault(c => c.CardData.Id == eventData.CardId);
                card.SetPlayable(false);
                card.Show();

                _handCards.Remove(card);
                RefreshSpline(handSpline, _handCards);

                switch (card.CardData.Class)
                {
                    default:
                    case CardClass.Brown:
                        DiscardCard(card);
                        break;
                    case CardClass.Blue:
                        if (card.CardData.Target == CardTarget.Self)
                        {
                            EquipCard(card);
                        }
                        else
                        {
                            var targetView = _playerViews.FirstOrDefault(p => p.PlayerId == eventData.TargetId);
                            targetView.EquipCard(card);
                        }
                        break;
                }
            }
        }

        private void OnTakingDamage(TakingDamageEventData eventData)
        {
            if (_player.ActorNumber != eventData.PlayerId) return;

            int currentHealth = _player.CurrentHealth;
            int newHealth = currentHealth - eventData.Amount;

            if (newHealth > _player.MaxHealth) return;

            if (eventData.Amount > 1)
            {
                StartCoroutine(LoseBulletRoutine(currentHealth, newHealth));
            }
            else
            {
                LoseBullet(newHealth);
            }
        }

        private void OnGainingHealth(GainingHealthEventData eventData)
        {
            if (_player.ActorNumber != eventData.PlayerId) return;

            int currentHealth = _player.CurrentHealth;
            int newHealth = currentHealth + eventData.Amount;

            if (newHealth > _player.MaxHealth) return;

            if (eventData.Amount > 1)
            {
                StartCoroutine(GainBulletRoutine(currentHealth, newHealth - 1));
            }
            else
            {
                GainBullet(newHealth - 1);
            }
        }

        private void OnDiscardingCard(DiscardingCardEventData eventData)
        {
            if (_player.ActorNumber != eventData.TargetId) return;

            var cardList = eventData.IsFromHand ? _handCards : _boardCards;
            var spline = eventData.IsFromHand ? handSpline : boardSpline;

            var card = cardList.SingleOrDefault(c => c.CardData.Id == eventData.CardId);
            card.SetPlayable(false);
            card.Show();

            cardList.Remove(card);
            RefreshSpline(spline, cardList);

            DiscardCard(card);
        }

        private void OnStealingCard(StealingCardEventData eventData)
        {
            if (_player.ActorNumber != eventData.TargetId) return;

            var cardData = _gameManager.Cards[eventData.CardId];
            var cardList = eventData.IsFromHand ? _handCards : _boardCards;
            var spline = eventData.IsFromHand ? handSpline : boardSpline;

            var card = cardList.SingleOrDefault(c => c.CardData.Id == cardData.Id);
            card.SetPlayable(false);
            card.Show();

            cardList.Remove(card);
            Destroy(card.gameObject);
            RefreshSpline(spline, cardList);

            var playerView = _playerViews.FirstOrDefault(p => p.PlayerId == eventData.PlayerId);
            playerView.DealCard(cardData, spline.transform);
        }

        #endregion
    }
}
