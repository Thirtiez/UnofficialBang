﻿using DG.Tweening;
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

        private PlayerView _currentPlayerView;

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

                int? range = null;

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

                _gameManager.CardSelected?.Invoke(new CardSelectedEventData { CardData = CardData, Range = range });
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
                    var playerView = hit.transform.GetComponent<PlayerView>();
                    if (playerView != null)
                    {
                        if (_currentPlayerView != null && _currentPlayerView != playerView)
                        {
                            _currentPlayerView.SetAreaReady(false);
                        }

                        _currentPlayerView = playerView;
                        _currentPlayerView.SetAreaReady(true);

                        SetReady(true);
                    }
                }
                else
                {
                    _currentPlayerView?.SetAreaReady(false);

                    SetReady(false);
                }
            }
        }

        protected void OnMouseUp()
        {
            if (_isPlayable && _isDragging)
            {
                _isDragging = false;

                if (_isReady && _currentPlayerView != null)
                {
                    SetReady(false);

                    _currentPlayerView.SetAreaReady(false);

                    _gameManager.SendEvent(PhotonEvent.CardPlaying, new CardPlayingEventData
                    {
                        InstigatorId = PhotonNetwork.LocalPlayer.ActorNumber,
                        TargetId = _currentPlayerView.PlayerId,
                        CardId = CardData.Id
                    });

                    Destroy(gameObject);
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
