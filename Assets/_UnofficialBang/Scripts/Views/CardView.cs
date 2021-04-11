using DG.Tweening;
using Photon.Pun;
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
        private bool _isDragging = false;

        private AreaView _currentAreaView;

        #endregion

        #region MonoBehaviour callbacks

        protected void OnMouseEnter()
        {
            if (!_isCovered && !_isAnimating && !_isDragging)
            {
                cardSpriteRenderer.gameObject.SetActive(false);

                _gameManager.CardHoverEnter?.Invoke(new CardHoverEnterEventData { CardView = this, IsPlayable = _isPlayable });
            }
        }

        protected void OnMouseExit()
        {
            if (!_isCovered && !_isAnimating && !_isDragging)
            {
                cardSpriteRenderer.gameObject.SetActive(true);

                _gameManager.CardHoverExit?.Invoke();
            }
        }

        protected void OnMouseDown()
        {
            if (_isPlayable && !_isAnimating && !_isDragging)
            {
                _isDragging = true;

                transform.rotation = Quaternion.identity;

                cardSpriteRenderer.gameObject.SetActive(true);

                int range = 0;

                switch (CardData.Target)
                {
                    case CardTarget.Range:
                        range = PhotonNetwork.LocalPlayer.Range;
                        break;

                    case CardTarget.FixedRange:
                        range = CardData.EffectValue.Value;
                        break;

                    case CardTarget.Anyone:
                        range = PhotonNetwork.CurrentRoom.PlayerCount;
                        break;
                }

                _gameManager.CardSelected?.Invoke(new SelectingCardEventData { CardData = CardData, Range = range });
            }
        }

        protected void OnMouseDrag()
        {
            if (_isPlayable && _isDragging)
            {
                var newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newPosition.z = -1f;

                transform.position = newPosition;

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("Area View"));
                if (hit.collider != null)
                {
                    var areaView = hit.transform.GetComponent<AreaView>();
                    if (areaView != null)
                    {
                        if (_currentAreaView != null && _currentAreaView != areaView)
                        {
                            _currentAreaView.SetReady(false);
                        }

                        _currentAreaView = areaView;
                        _currentAreaView.SetReady(true);

                        SetReady(true);
                    }
                }
                else
                {
                    _currentAreaView?.SetReady(false);

                    SetReady(false);
                }
            }
        }

        protected void OnMouseUp()
        {
            if (_isPlayable && _isDragging)
            {
                _isDragging = false;

                if (_isReady && _currentAreaView != null)
                {
                    SetReady(false);

                    _currentAreaView.SetReady(false);

                    var eventData = new PlayingCardEventData
                    {
                        InstigatorId = PhotonNetwork.LocalPlayer.ActorNumber,
                        TargetId = _currentAreaView.TargetId,
                        CardId = CardData.Id
                    };

                    if (CardData.Target == CardTarget.Everyone)
                    {
                        eventData.TargetId = PhotonNetwork.LocalPlayer.ActorNumber;
                    }
                    else if (CardData.Target == CardTarget.EveryoneElse)
                    {
                        eventData.TargetId = _gameManager.NextLivingPlayerId;
                    }

                    _gameManager.SendEvent(PhotonEvent.PlayingCard, eventData);
                }
                else
                {
                    MoveTo(_snappedPosition, _snappedRotation, _snappedScale);
                }

                _gameManager.CardCanceled?.Invoke();
            }
        }

        #endregion

        #region Private methods

        private void SetReady(bool isReady)
        {
            _isReady = isReady;

            highlightSpriteRenderer.color = isReady ? _gameManager.ColorSettings.CardReady : _gameManager.ColorSettings.CardPlayable;
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
                Show();
            }

            highlightSpriteRenderer.color = _gameManager.ColorSettings.CardPlayable;
            highlightSpriteRenderer.gameObject.SetActive(false);
        }

        public void Show()
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
