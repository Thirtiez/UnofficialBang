using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class AreaView : BaseView
    {
        #region Inspector fields

        [Header("View")]

        [SerializeField]
        private BaseView view;

        [Header("Area")]

        [SerializeField]
        private SpriteRenderer areaMask;

        [SerializeField]
        private Collider2D areaCollider;

        #endregion

        #region 

        public int? TargetId => view is PlayerView playerView ? playerView.PlayerId : (int?)null;

        #endregion

        #region Private fields

        private GameManager _gameManager;

        #endregion

        #region Monobehaviour callbacks

        protected void OnEnable()
        {
            _gameManager = GameManager.Instance;

            _gameManager.CardSelected += OnCardSelected;
            _gameManager.CardCanceled += OnCardCanceled;

            SetPlayable(false);
            SetReady(false);
        }

        protected void OnDisable()
        {
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
        }

        #endregion

        #region Public methods

        public void SetReady(bool isReady)
        {
            areaMask.color = isReady ? _gameManager.ColorSettings.AreaReady : _gameManager.ColorSettings.AreaPlayable;
        }

        #endregion

        #region Private methods

        private void SetPlayable(bool isPlayable)
        {
            areaMask.enabled = isPlayable;
            areaCollider.enabled = isPlayable;
        }

        #endregion

        #region Event handlers

        private void OnCardSelected(CardSelectedEventData eventData)
        {
            var card = eventData.CardData;

            if (view is PlayerView playerView)
            {
                int distance = playerView.PlayerDistance;
                if (distance == 0)
                {
                    SetPlayable(card.Class == CardClass.Blue && card.Target == CardTarget.Self);
                }
                else if (distance <= eventData.Range)
                {
                    var player = PhotonNetwork.CurrentRoom.GetPlayer(playerView.PlayerId);

                    SetPlayable(distance + player.BonusDistance <= eventData.Range);
                }
            }
            else if (view is DeckView)
            {
                SetPlayable(card.Class == CardClass.Brown &&
                    (card.Target == CardTarget.Everyone || card.Target == CardTarget.EveryoneElse || card.Target == CardTarget.Self));
            }

            areaMask.color = _gameManager.ColorSettings.AreaPlayable;
        }

        private void OnCardCanceled()
        {
            SetPlayable(false);
        }

        #endregion

    }
}