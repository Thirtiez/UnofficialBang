using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class CardView : MonoBehaviour
    {
        #region Inspector fields

        [SerializeField]
        private SpriteRenderer cardSpriteRenderer;

        [SerializeField]
        private SpriteRenderer highlightSpriteRenderer;

        [SerializeField]
        private Sprite cardBack;

        [SerializeField]
        private Sprite roleBack;

        #endregion

        #region Public properties

        public CardData CardData { get; private set; }

        #endregion

        #region Private fields

        private GameManager _gameManager;

        private bool _isCovered = false;
        private bool _isAnimating = false;
        private bool _isPlayable = false;

        #endregion

        #region MonoBehaviour callbacks

        protected void OnMouseEnter()
        {
            if (!_isCovered && !_isAnimating)
            {
                _gameManager.CardMouseOverEnter(new CardMouseOverEnterEventData { CardView = this, IsPlayable = _isPlayable});

                cardSpriteRenderer.gameObject.SetActive(false);
            }
        }

        protected void OnMouseExit()
        {
            if (!_isCovered && !_isAnimating)
            {
                _gameManager.CardMouseOverExit();

                cardSpriteRenderer.gameObject.SetActive(true);
            }
        }

        protected void OnMouseDown()
        {
            if (_isPlayable)
            {
                // TODO OnMouseDown

                Debug.Log("OnMouseDown");
            }
        }

        protected void OnMouseDrag()
        {
            if (_isPlayable)
            {
                // TODO OnMouseDrag

                Debug.Log("OnMouseDrag");
            }
        }

        protected void OnMouseUp()
        {
            if (_isPlayable)
            {
                // TODO OnMouseUp

                Debug.Log("OnMouseUp");
            }
        }

        #endregion

        #region Public methods

        public void Configure(CardData cardData, bool isCovered)
        {
            _gameManager = GameManager.Instance;
            _isCovered = isCovered;

            CardData = cardData;

            if (isCovered)
            {
                Hide();
            }
            else
            {
                Reveal();
            }

            highlightSpriteRenderer.color = _gameManager.ColorSettings.CardHighlight;
            highlightSpriteRenderer.gameObject.SetActive(false);
        }

        public void Reveal()
        {
            _isCovered = false;

            cardSpriteRenderer.sprite = _gameManager.CardSpriteTable.Get(CardData.Sprite);
        }

        public void Hide()
        {
            _isCovered = true;

            cardSpriteRenderer.sprite = CardData.Class == CardClass.Role ? roleBack : cardBack;
        }

        public void MoveTo(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _isAnimating = true;

            float duration = _gameManager.AnimationSettings.DealCardDuration;

            transform
                .DOLocalMove(position, duration)
                .SetEase(Ease.OutQuint);
            transform
                .DOLocalRotateQuaternion(rotation, duration)
                .SetEase(Ease.OutQuint);
            transform
                .DOScale(Vector3.one, duration)
                .SetEase(Ease.OutQuint)
                .OnComplete(() => _isAnimating = false);
        }

        public void SetPlayable(bool isPlayable)
        {
            _isPlayable = isPlayable;

            highlightSpriteRenderer.gameObject.SetActive(isPlayable);
        }

        #endregion
    }
}
