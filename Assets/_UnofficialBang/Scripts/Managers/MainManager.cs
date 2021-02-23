using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Thirties.UnofficialBang
{
    public class MainManager : MonoBehaviourPunCallbacks
    {
        #region Inspector fields

        [Header("Configuration")]

        [SerializeField]
        private int minPlayerCount = 3;

        [SerializeField]
        private NicknameDataTable nicknameDataTable;

        [Header("Room Panel")]

        [SerializeField]
        private GameObject roomListingPanel;

        [SerializeField]
        private Transform roomsContainer;

        [SerializeField]
        private RoomElementUI roomElementPrefab;

        [SerializeField]
        private TMP_InputField nicknameInputField;

        [SerializeField]
        private Button createRoomButton;

        [SerializeField]
        private Button refreshButton;

        [Header("Player Panel")]

        [SerializeField]
        private GameObject playerListingPanel;

        [SerializeField]
        private Transform playersContainer;

        [SerializeField]
        private PlayerElementUI playerElementPrefab;

        [SerializeField]
        private TMP_Text roomNameText;

        [SerializeField]
        private Button leaveButton;

        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private Button startButton;

        [Header("Create Room Modal")]

        [SerializeField]
        private GameObject createRoomModal;

        [SerializeField]
        private TMP_InputField roomNameInputField;

        [SerializeField]
        private Button cancelRoomButton;

        [SerializeField]
        private Button confirmRoomButton;

        #endregion

        #region Private fields

        private List<RoomElementUI> roomElements = new List<RoomElementUI>();
        private List<PlayerElementUI> playerElements = new List<PlayerElementUI>();

        #endregion

        #region Monobehaviour callbacks

        protected void Start()
        {
#if UNITY_EDITOR
            int nameIndex = Random.Range(0, nicknameDataTable.Records.Count);
            nicknameInputField.text = nicknameDataTable.Records[nameIndex].Nickname;
#else
            if (PlayerPrefs.HasKey("nickname"))
            {
                nicknameInputField.text = PlayerPrefs.GetString("nickname");
            }
#endif

            roomListingPanel.SetActive(false);
            playerListingPanel.SetActive(false);
            createRoomModal.SetActive(false);

            nicknameInputField.onValueChanged.AddListener(OnNicknameInputFieldValueChanged);
            roomNameInputField.onValueChanged.AddListener(OnRoomNameInputFieldValueChanged);

            createRoomButton.onClick.AddListener(OnCreateRoomButtonClick);
            cancelRoomButton.onClick.AddListener(OnCancelRoomButtonClick);
            confirmRoomButton.onClick.AddListener(OnConfirmRoomButtonClick);
            refreshButton.onClick.AddListener(OnRefreshButtonClick);
            leaveButton.onClick.AddListener(OnLeaveButtonClick);
            readyButton.onClick.AddListener(OnReadyButtonClick);
            startButton.onClick.AddListener(OnStartButtonClick);

            PhotonNetwork.ConnectUsingSettings();
        }

        #endregion

        #region PUN callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log($"Connected to master");

            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"Disconnetted: {cause}");

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"Lobby joined");

            roomListingPanel.SetActive(true);
            playerListingPanel.SetActive(false);
            createRoomModal.SetActive(false);

            createRoomButton.interactable = !string.IsNullOrEmpty(nicknameInputField.text);

            roomElements.ForEach(re => Destroy(re.gameObject));
            roomElements.Clear();

            playerElements.ForEach(pe => Destroy(pe.gameObject));
            playerElements.Clear();

            SetReady(false);

            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Room joined");

            roomListingPanel.SetActive(false);
            playerListingPanel.SetActive(true);
            createRoomModal.SetActive(false);

            readyButton.interactable = true;

            roomNameText.text = PhotonNetwork.CurrentRoom.Name;

            PhotonNetwork.PlayerList.ToList().ForEach(p =>
            {
                var playerElement = Instantiate(playerElementPrefab, playersContainer);
                playerElement.Configure(p);

                playerElements.Add(playerElement);
            });

            RefreshStartButton();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Error {returnCode}: {message}");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("Room list update received");

            roomList.ForEach(ri =>
            {
                var room = roomElements.SingleOrDefault(re => re.RoomInfo.Name == ri.Name);
                if (room != null)
                {
                    if (ri.RemovedFromList)
                    {
                        roomElements.Remove(room);

                        Destroy(room.gameObject);
                    }
                    else
                    {
                        room.Refresh(ri);
                    }
                }
                else if (!ri.RemovedFromList)
                {
                    var roomElement = Instantiate(roomElementPrefab, roomsContainer);
                    roomElement.Configure(ri, () => OnJoinButtonClick(ri.Name));

                    roomElements.Add(roomElement);
                }
            });
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player {newPlayer.NickName} entered room");

            bool playerFound = playerElements.Any(pe => pe.Player.ActorNumber == newPlayer.ActorNumber);
            if (!playerFound)
            {
                var playerElement = Instantiate(playerElementPrefab, playersContainer);
                playerElement.Configure(newPlayer);

                playerElements.Add(playerElement);
            }

            RefreshStartButton();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player {otherPlayer.NickName} left room");

            var playerElement = playerElements.SingleOrDefault(pe => pe.Player.ActorNumber == otherPlayer.ActorNumber);
            if (playerElement != null)
            {
                playerElements.Remove(playerElement);

                Destroy(playerElement.gameObject);
            }

            RefreshStartButton();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log($"Player {targetPlayer.NickName} properties updated");

            RefreshPlayerElement(targetPlayer);
            RefreshStartButton();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"Master client switched to {newMasterClient.NickName}");

            RefreshPlayerElement(newMasterClient);
            RefreshStartButton();
        }

        #endregion

        #region Listeners

        private void OnNicknameInputFieldValueChanged(string value)
        {
            createRoomButton.interactable = !string.IsNullOrEmpty(value);

            roomElements?.ForEach(re => re.SetInteractable(value));
        }

        private void OnRoomNameInputFieldValueChanged(string value)
        {
            confirmRoomButton.interactable = !string.IsNullOrEmpty(value);
        }

        private void OnCreateRoomButtonClick()
        {
            roomListingPanel.SetActive(true);
            playerListingPanel.SetActive(false);
            createRoomModal.SetActive(true);

            roomNameInputField.text = $"Stanza di {nicknameInputField.text}";
        }

        private void OnCancelRoomButtonClick()
        {
            roomListingPanel.SetActive(true);
            playerListingPanel.SetActive(false);
            createRoomModal.SetActive(false);
        }

        private void OnConfirmRoomButtonClick()
        {
            var roomOptions = new RoomOptions
            {
                MaxPlayers = 7,
                BroadcastPropsChangeToAll = true,
            };

            PhotonNetwork.NickName = nicknameInputField.text;
            PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        }

        private void OnRefreshButtonClick()
        {
            PhotonNetwork.JoinLobby();
        }

        private void OnJoinButtonClick(string roomName)
        {
#if !UNITY_EDITOR
            PlayerPrefs.SetString("nickname", nicknameInputField.text);
#endif
            PhotonNetwork.NickName = nicknameInputField.text;
            PhotonNetwork.JoinRoom(roomName);
        }

        private void OnReadyButtonClick()
        {
            SetReady(true);
        }

        private void OnLeaveButtonClick()
        {
            PhotonNetwork.LeaveRoom();
        }

        private void OnStartButtonClick()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel("Game");
        }

        #endregion

        #region Private methods

        private void SetReady(bool isReady)
        {
            var customProperties = new Hashtable();
            customProperties["ready"] = isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

            readyButton.interactable = !isReady;
        }

        private void RefreshPlayerElement(Player player)
        {
            var playerElement = playerElements.SingleOrDefault(pe => pe.Player.ActorNumber == player.ActorNumber);
            if (playerElement != null)
            {
                playerElement.Refresh(player);
            }
        }

        private void RefreshStartButton()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startButton.gameObject.SetActive(true);

                int readyCount = playerElements.Count(pe => (bool)pe.Player.CustomProperties["ready"]);
                startButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= minPlayerCount && PhotonNetwork.CurrentRoom.PlayerCount <= readyCount;
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}