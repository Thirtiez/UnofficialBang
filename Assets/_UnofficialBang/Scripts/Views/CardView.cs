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

        private Vector3 _snappedPosition;
        private Quaternion _snappedRotation;
        private Vector3 _snappedScale;

        private bool _isCovered = false;
        private bool _isAnimating = false;
        private bool _isPlayable = false;
        private bool _isReady = false;

        #endregion

        #region MonoBehaviour callbacks

        protected void OnMouseEnter()
        {
            if (!_isCovered && !_isAnimating)
            {
                cardSpriteRenderer.gameObject.SetActive(false);

                _gameManager.CardMouseOverEnter(new CardMouseOverEnterEventData { CardView = this, IsPlayable = _isPlayable });
            }
        }

        protected void OnMouseExit()
        {
            if (!_isCovered && !_isAnimating)
            {
                cardSpriteRenderer.gameObject.SetActive(true);

                _gameManager.CardMouseOverExit();
            }
        }

        protected void OnMouseDown()
        {
            if (_isPlayable)
            {
                _isAnimating = true;

                transform.rotation = Quaternion.identity;

                cardSpriteRenderer.gameObject.SetActive(true);

                _gameManager.CardSelected(new CardSelectedEventData { CardData = CardData });
            }
        }

        protected void OnMouseDrag()
        {
            if (_isPlayable)
            {
                var newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newPosition.z = -1f;

                transform.position = newPosition;

                // TODO targeting
            }
        }

        protected void OnMouseUp()
        {
            if (_isPlayable)
            {
                _isAnimating = false;

                if (_isReady)
                {
                    // TODO resolution

                    _isReady = false;
                }
                else
                {
                    MoveTo(_snappedPosition, _snappedRotation, _snappedScale);

                    _gameManager.CardCanceled();
                }
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

            highlightSpriteRenderer.color = _gameManager.ColorSettings.CardPlayable;
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

            transform.position = new Vector3(transform.position.x, transform.position.y, position.z);

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

            _snappedPosition = position;
            _snappedRotation = rotation;
            _snappedScale = scale;
        }

        public void SetPlayable(bool isPlayable)
        {
            _isPlayable = isPlayable;

            highlightSpriteRenderer.gameObject.SetActive(isPlayable);
        }

        #endregion
    }
}
