using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class PlayerView : MonoBehaviour
    {
        #region Inspector fields

        [Header("Parameters")]

        [SerializeField]
        private int playerNumber;

        [SerializeField]
        [SuffixLabel("s")]
        private float cardAnimationDuration = 1.0f;

        [SerializeField]
        [SuffixLabel("s")]
        private float bulletAnimationDuration = 0.5f;

        [SerializeField]
        [SuffixLabel("s")]
        private float bulletAnimationDelay = 0.1f;

        [SerializeField]
        [Range(0, 1)]
        private float cardPreferredDistance = 0.2f;

        [SerializeField]
        private CardView cardPrefab;

        [Header("References")]

        [SerializeField]
        private Transform deckTransform;

        [SerializeField]
        private Spline sideSpline;

        [SerializeField]
        private Spline handSpline;

        [SerializeField]
        private Spline boardSpline;

        [SerializeField]
        private TMP_Text nicknameText;

        [SerializeField]
        private List<CanvasGroup> bulletCanvasGroups;

        #endregion

        #region Private fields

        private List<CardView> _sideCards = new List<CardView>();
        private List<CardView> _handCards = new List<CardView>();
        private List<CardView> _boardCards = new List<CardView>();

        private GameManager _gameManager;
        private Player _player;

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

            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
            _gameManager.CharactersDealt += OnCharactersDealt;

            Configure();
        }

        protected void OnDisable()
        {
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
            _gameManager.CharactersDealt -= OnCharactersDealt;
        }

        #endregion

        #region Private methods

        private void Configure()
        {
            int playerCount = _gameManager.Players.Count;

            bool isActive = EnableTable[playerCount].Contains(playerNumber);
            gameObject.SetActive(isActive);

            if (isActive)
            {
                int localPlayerIndex = _gameManager.Players.IndexOf(PhotonNetwork.LocalPlayer);
                int playerOffset = EnableTable[playerCount].IndexOf(playerNumber);
                int playerIndex = (localPlayerIndex + playerOffset) % playerCount;
                _player = _gameManager.Players[playerIndex];

                nicknameText.text = _player.NickName;

                bulletCanvasGroups.ForEach(b => b.alpha = 0);
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

                cardList[i].transform.DOLocalMove(position, cardAnimationDuration).SetEase(Ease.OutQuint);
                cardList[i].transform.DOLocalRotateQuaternion(rotation, cardAnimationDuration).SetEase(Ease.OutQuint);
                cardList[i].transform.DOScale(Vector3.one, cardAnimationDuration).SetEase(Ease.OutQuint);
            }
        }

        private IEnumerator GainBulletRoutine(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GainBullet(bulletCanvasGroups[i]);
                yield return new WaitForSeconds(bulletAnimationDelay);
            }
        }

        private void GainBullet(CanvasGroup bullet)
        {
            bullet.transform.DOScale(Vector3.one * 0.1f, bulletAnimationDuration).SetEase(Ease.OutBack);
            bullet.DOFade(1, bulletAnimationDuration);
        }

        private void LoseBullet(CanvasGroup bullet)
        {
            bullet.transform.DOScale(Vector3.zero, bulletAnimationDuration).SetEase(Ease.InBack);
            bullet.DOFade(0, bulletAnimationDuration);
        }

        #endregion

        #region Event handlers

        private void OnCardDealing(CardDealingEventData eventData)
        {
            if (eventData.PlayerId == _player.ActorNumber)
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
            if (eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber && eventData.PlayerId == _player.ActorNumber)
            {
                _sideCards[0].Reveal();
            }
        }

        private void OnCharactersDealt()
        {
            int bulletCount = _gameManager.GetPlayerProperties(_player).CurrentHealth;
            StartCoroutine(GainBulletRoutine(bulletCount));
        }

        #endregion
    }
}
