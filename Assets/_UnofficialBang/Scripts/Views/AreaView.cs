using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    public class AreaView : MonoBehaviour
    {
        #region Inspector fields

        [Header("View")]

        [SerializeField]
        private PlayerView playerView;

        [Header("Area")]

        [SerializeField]
        private SpriteRenderer areaMask;

        [SerializeField]
        private Collider2D areaCollider;

        #endregion

        #region Public properties

        public int TargetId => playerView.PlayerId;

        #endregion

        #region Private fields

        private GameManager _gameManager;

        #endregion

        #region Monobehaviour callbacks

        protected void OnEnable()
        {
            _gameManager = GameManager.Instance;

            _gameManager.StateEnter += OnStateEnter;

            _gameManager.CardSelected += OnCardSelected;
            _gameManager.CardCanceled += OnCardCanceled;

            SetPlayable(false);
            SetReady(false);
        }

        protected void OnDisable()
        {
            _gameManager.StateEnter -= OnStateEnter;
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
        }

        #endregion

        #region Public methods

        public void SetReady(bool isReady)
        {
            if (playerView.IsCurrentPlayer) return;

            areaMask.color = isReady ? _gameManager.ColorSettings.AreaReady : _gameManager.ColorSettings.AreaPlayable;
        }

        #endregion

        #region Private methods

        private void SetPlayable(bool isPlayable)
        {
            areaMask.enabled = playerView.IsCurrentPlayer || isPlayable;
            areaCollider.enabled = isPlayable;
        }

        private void SetColor(bool isCurrentPlayer)
        {
            areaMask.enabled = isCurrentPlayer;
            areaMask.color = isCurrentPlayer ? _gameManager.ColorSettings.AreaTurn : _gameManager.ColorSettings.AreaPlayable;
        }

        #endregion

        #region Event handlers

        private void OnStateEnter(BaseState state)
        {
            SetColor((state is CardSelectionState && playerView.IsCurrentPlayer) || (state is CardResolutionState && playerView.IsCurrentTarget));
        }

        private void OnCardSelected(CardSelectedEventData eventData)
        {
            int distance = playerView.PlayerDistance;

            if (playerView.IsCurrentPlayer)
            {
                SetPlayable(eventData.Range == 0);
            }
            else
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(playerView.PlayerId);
                SetPlayable(player.IsAlive && distance + player.BonusDistance <= eventData.Range);
            }
        }

        private void OnCardCanceled()
        {
            SetPlayable(false);
        }

        #endregion

    }
}