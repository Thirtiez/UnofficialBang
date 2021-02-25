using DG.Tweening;
using Photon.Pun;
using Sirenix.OdinInspector;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class PlayerView : MonoBehaviour
    {
        #region Inspector fields

        [Header("Parameters")]

        [SerializeField]
        [SuffixLabel("s")]
        private float tweenDuration = 1.0f;

        [SerializeField]
        [Range(0, 1)]
        private float preferredDistance = 0.2f;

        [Header("Prefabs")]

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

        #endregion

        #region Private fields

        private List<CardView> _handCards = new List<CardView>();
        private List<CardView> _boardCards = new List<CardView>();
        private List<CardView> _sideCards = new List<CardView>();

        private GameManager _gameManager;

        #endregion

        #region Monobehaviour callbacks

        protected void OnEnable()
        {
            _gameManager = GameManager.Instance;

            _gameManager.CardDealing += OnCardDealing;
            _gameManager.RoleRevealing += OnRoleRevealing;
        }

        protected void OnDisable()
        {
            _gameManager.CardDealing -= OnCardDealing;
            _gameManager.RoleRevealing -= OnRoleRevealing;
        }

        #endregion

        #region Private methods

        private void Deal(CardData cardData, Spline target, List<CardView> cardList)
        {
            var card = Instantiate(cardPrefab, deckTransform.position, deckTransform.rotation, target.transform);
            card.Configure(cardData);

            cardList.Add(card);

            float possibleDistance = 1f / cardList.Count;
            float distance = possibleDistance >= preferredDistance ? preferredDistance : possibleDistance;
            float startTime = (1 - (distance * (cardList.Count - 1))) * 0.5f;

            for (int i = 0; i < cardList.Count; i++)
            {
                var curve = sideSpline.GetSample(startTime + distance * i);
                var rotation = Quaternion.LookRotation(Vector3.forward, curve.up);

                cardList[i].transform.DOLocalMove(curve.location, tweenDuration).SetEase(Ease.OutQuint);
                cardList[i].transform.DOLocalRotateQuaternion(rotation, tweenDuration).SetEase(Ease.OutQuint);
            }
        }

        #endregion

        #region Event handlers

        private void OnCardDealing(CardDealingEventData eventData)
        {
            if (eventData.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var cardData = _gameManager.Cards[eventData.CardId];

                if (cardData.Class == CardClass.Blue || cardData.Class == CardClass.Brown)
                {
                    Deal(cardData, handSpline, _handCards);
                }
                else if (cardData.Class == CardClass.Character || cardData.Class == CardClass.Role)
                {
                    Deal(cardData, sideSpline, _sideCards);
                }
            }
            else
            {
                //TODO deal to opponent
            }
        }

        private void OnRoleRevealing(RoleRevealingEventData eventData)
        {
            if (eventData.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                var cardData = _gameManager.Cards[eventData.CardId];
                var player = PhotonNetwork.CurrentRoom.GetPlayer(eventData.PlayerId);

                //TODO reveal opponent
            }
        }

        #endregion
    }
}
