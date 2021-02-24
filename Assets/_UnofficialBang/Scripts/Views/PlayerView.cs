using DG.Tweening;
using Sirenix.OdinInspector;
using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class PlayerView : MonoBehaviour
    {
        [Header("Parameters")]

        [SerializeField]
        [SuffixLabel("s")]
        private float tweenDuration;

        [Header("Cards")]

        [SerializeField]
        private CardView cardPrefab;

        [Header("Deck")]

        [SerializeField]
        private Transform deckTransform;

        [Header("Card containers")]

        [SerializeField]
        private Spline sideSpline;

        [SerializeField]
        private Spline handSpline;

        [SerializeField]
        private Spline boardSpline;

        private List<CardView> _handCards = new List<CardView>();
        private List<CardView> _boardCards = new List<CardView>();
        private CardView _characterCard;
        private CardView _roleCard;

        public void DealRole(CardData cardData)
        {
            _roleCard = Deal(cardData, sideSpline);
        }

        public void DealCharacter(CardData cardData)
        {
            _characterCard = Deal(cardData, sideSpline);
        }

        public void DealPlayingCard(CardData cardData)
        {
            var card = Deal(cardData, handSpline);
            _handCards.Add(card);
        }

        private CardView Deal(CardData cardData, Spline target)
        {
            var card = Instantiate(cardPrefab, deckTransform.position, deckTransform.rotation, target.transform);
            card.Configure(cardData);

            var curve = sideSpline.GetSample(0.4f);
            card.transform.DOLocalMove(curve.location, tweenDuration).SetEase(Ease.OutQuint);

            var rotation = Quaternion.LookRotation(Vector3.forward, curve.up);
            card.transform.DOLocalRotateQuaternion(rotation, tweenDuration).SetEase(Ease.OutQuint);

            return card;
        }
    }
}
