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
            SetVisible(false);
        }

        protected void OnDisable()
        {
            _gameManager.StateEnter -= OnStateEnter;
            _gameManager.CardSelected -= OnCardSelected;
            _gameManager.CardCanceled -= OnCardCanceled;
        }

        #endregion

        #region Public methods

        public void SetHighlight(bool value)
        {
            if (playerView.IsCurrentPlayerOrTarget) return;

            SetColor(value ? _gameManager.ColorSettings.AreaReady : _gameManager.ColorSettings.AreaPlayable);
        }

        #endregion

        #region Private methods

        private void SetVisible(bool value)
        {
            areaMask.enabled = value;
        }

        private void SetPlayable(bool isPlayable)
        {
            areaCollider.enabled = isPlayable;
        }

        private void SetColor(Color color)
        {
            areaMask.color = color;
        }

        #endregion

        #region Event handlers

        private void OnStateEnter(BaseState state)
        {
            SetColor(playerView.IsCurrentPlayerOrTarget ? _gameManager.ColorSettings.AreaTurn : _gameManager.ColorSettings.AreaPlayable);
            SetVisible(playerView.IsCurrentPlayerOrTarget);
        }

        private void OnCardSelected(SelectingCardEventData eventData)
        {
            int distance = playerView.PlayerDistance;

            if (playerView.IsCurrentPlayerOrTarget)
            {
                SetPlayable(eventData.Range == 0);
            }
            else
            {
                var player = PhotonNetwork.CurrentRoom.GetPlayer(playerView.PlayerId);
                bool isPlayable = player.IsAlive && distance + player.BonusDistance <= eventData.Range;
                SetPlayable(isPlayable);
                SetVisible(isPlayable);
            }
        }

        private void OnCardCanceled()
        {
            SetPlayable(false);
            SetVisible(playerView.IsCurrentPlayerOrTarget);
        }

        #endregion

    }
}