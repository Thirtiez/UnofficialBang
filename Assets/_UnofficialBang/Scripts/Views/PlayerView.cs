using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
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

        [SerializeField]
        private Transform deckTransform;

        [Header("Area")]

        [SerializeField]
        private SpriteRenderer areaMask;

        [SerializeField]
        private Collider2D areaCollider;

        #endregion

        #region Public properties

        public int PlayerId { get; private set; }

        #endregion

        #region Private fields

        private List<CardView> _sideCards = new List<CardView>();
        private List<CardView> _handCards = new List<CardView>();
        private List<CardView> _boardCards = new List<CardView>();

        private GameManager _gameManager;
        private int _playerDistance;

        private bool _isPlayable = false;

        private CardView _roleCard => _sideCards[0];
        private CardView _characterCard => _sideCards[1];

        #endregion

        #region Constants

        private readonly Dictionary<int, List<int>> EnableTable = new Dictionary<int, List<int>>
        {
            { 1, new List<int> {0} },
            { 2, new List<int> {0, 4} },
            { 3, new List<int> {0, 2, 6} },
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
            _gameManager = GameManager.Instance;

            _gameManager.OnStateEnter += OnStateEnter;
            _gameManager.OnStateExit += OnStateExit;
            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
            _gameManager.CardSelected += OnCardSelected;
            _gameManager.CardCanceled += OnCardCanceled;
        }

        protected void OnDisable()
        {
            _gameManager.OnStateEnter -= OnStateEnter;
            _gameManager.OnStateExit -= OnStateExit;
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
        }

        #endregion

        #region Public methods

        public void SetAreaReady(bool isReady)
        {
            areaMask.color = isReady ? _gameManager.ColorSettings.AreaReady : _gameManager.ColorSettings.AreaPlayable;
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

                _playerDistance = playerOffset > playerCount / 2 ? playerCount - playerOffset : playerOffset;
                PlayerId = PhotonNetwork.CurrentRoom.TurnPlayerIds[playerIndex];
                nicknameText.text = PhotonNetwork.CurrentRoom.GetPlayer(PlayerId).NickName;

                bullets.ForEach(b =>
                {
                    b.gameObject.SetActive(false);
                    b.transform.localScale = Vector3.zero;
                });

                areaMask.gameObject.SetActive(false);
                areaCollider.enabled = false;
            }
        }

        private void DealCard(CardData cardData, Spline targetSpline, List<CardView> cardList, bool isCovered = false)
        {
            var card = Instantiate(cardPrefab, deckTransform.position, deckTransform.rotation, targetSpline.transform);
            card.Configure(cardData, isCovered);

            cardList.Add(card);

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

        private IEnumerator GainBulletRoutine(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GainBullet(bullets[i]);
                yield return new WaitForSeconds(_gameManager.AnimationSettings.BulletAnimationDelay);
            }
        }

        private void GainBullet(GameObject bullet)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform
                .DOScale(Vector3.one * 0.1f, _gameManager.AnimationSettings.BulletAnimationDuration)
                .SetEase(Ease.OutBack);
        }

        private void LoseBullet(GameObject bullet)
        {
            bullet.transform
                .DOScale(Vector3.zero, _gameManager.AnimationSettings.BulletAnimationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => bullet.gameObject.SetActive(false));
        }

        private void ConfigurePlayableCards()
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

            _characterCard.SetPlayable(_characterCard.CardData.Effect == CardEffect.SidKetchum);
        }

        private void SetAreaPlayable(bool isPlayable)
        {
            _isPlayable = isPlayable;

            areaMask.gameObject.SetActive(_isPlayable);
            areaCollider.enabled = _isPlayable;
        }

        #endregion

        #region Event handlers

        private void OnStateEnter(BaseState state)
        {
            if (state is RolesDealingState)
            {
                Configure();
            }
            else if (state is CardSelectionState && PlayerId == PhotonNetwork.LocalPlayer.ActorNumber && _gameManager.IsLocalPlayerTurn)
            {
                ConfigurePlayableCards();
            }
        }

        private void OnStateExit(BaseState state)
        {
            if (state is CharactersDealingState)
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(PlayerId);
                StartCoroutine(GainBulletRoutine(player.CurrentHealth));
            }
        }

        private void OnCardDealing(CardDealingEventData eventData)
        {
            if (eventData.PlayerId == PlayerId)
            {
                var cardData = _gameManager.Cards[eventData.CardId];

                if (cardData.Class == CardClass.Blue || cardData.Class == CardClass.Brown)
                {
                    bool isCovered = eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber;
                    DealCard(cardData, handSpline, _handCards, isCovered);
                }
                else if (cardData.Class == CardClass.Character)
                {
                    DealCard(cardData, sideSpline, _sideCards);
                }
                else if (cardData.Class == CardClass.Role)
                {
                    bool isCovered = eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber;
                    DealCard(cardData, sideSpline, _sideCards, isCovered);
                }
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            if (eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber && eventData.PlayerId == PlayerId)
            {
                _roleCard.Reveal();
            }
        }

        private void OnCardSelected(CardSelectedEventData eventData)
        {
            var card = eventData.CardData;
            if (playerNumber == 0)
            {
                if (card.Class == CardClass.Blue && card.Target == CardTarget.Self)
                {
                    SetAreaPlayable(true);
                }
            }
            else if (_playerDistance <= eventData.Range)
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(PlayerId);

                SetAreaPlayable(_playerDistance + player.BonusDistance <= eventData.Range);
            }

            areaMask.color = _gameManager.ColorSettings.AreaPlayable;
        }

        private void OnCardCanceled()
        {
            SetAreaPlayable(false);
        }

        #endregion
    }
}
