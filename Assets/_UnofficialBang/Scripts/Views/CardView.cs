﻿using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Sprite cardBack;

        [SerializeField]
        private Sprite roleBack;

        [SerializeField]
        [SuffixLabel("s")]
        private float animationDuration = 1.0f;

        public CardData CardData { get; private set; }

        private bool isCovered = false;
        private bool isAnimating = false;

        protected void OnMouseEnter()
        {
            if (!isCovered && !isAnimating)
            {
                GameManager.Instance.CardMouseOverEnter(this);

                spriteRenderer.enabled = false;
            }
        }

        protected void OnMouseExit()
        {
            if (!isCovered && !isAnimating)
            {
                GameManager.Instance.CardMouseOverExit();

                spriteRenderer.enabled = true;
            }
        }

        public void Configure(CardData cardData, bool isCovered)
        {
            CardData = cardData;

            this.isCovered = isCovered;

            if (isCovered)
            {
                Hide();
            }
            else
            {
                Reveal();
            }
        }

        public void Reveal()
        {
            isCovered = false;

            spriteRenderer.sprite = GameManager.Instance?.CardSpriteTable?.Get(CardData.Sprite);
        }

        public void Hide()
        {
            isCovered = true;

            spriteRenderer.sprite = CardData.Class == CardClass.Role ? roleBack : cardBack;
        }

        public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            isAnimating = true;

            transform
                .DOLocalMove(position, animationDuration)
                .SetEase(Ease.OutQuint);
            transform
                .DOLocalRotateQuaternion(rotation, animationDuration)
                .SetEase(Ease.OutQuint);
            transform
                .DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutQuint)
                .OnComplete(() => isAnimating = false);
        }
    }
}
