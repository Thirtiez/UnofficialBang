using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainMenu : MonoBehaviourPunCallbacks
{
    #region Inspector fields

    [Header("Room Panel")]

    [SerializeField]
    private GameObject roomListingPanel;

    [SerializeField]
    private Transform roomsContainer;

    [SerializeField]
    private RoomElement roomElementPrefab;

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
    private PlayerElement playerElementPrefab;

    [SerializeField]
    private TMP_Text roomNameText;

    [SerializeField]
    private Button leaveButton;

    [SerializeField]
    private Button readyButton;

    [Header("Create Room Modal")]

    [SerializeField]
    private GameObject createRoomModal;

    [SerializeField]
    private TMP_InputField roomNameInputField;

    [SerializeField]
    private Button confirmRoomButton;

    #endregion

    #region Private fields

    private List<RoomElement> roomElements = new List<RoomElement>();
    private List<PlayerElement> playerElements = new List<PlayerElement>();

    #endregion

    #region Monobehaviour callbacks

    protected void Start()
    {
        if (PlayerPrefs.HasKey("nickname"))
        {
            nicknameInputField.text = PlayerPrefs.GetString("nickname");
        }

        roomListingPanel.SetActive(false);
        playerListingPanel.SetActive(false);
        createRoomModal.SetActive(false);

        nicknameInputField.onValueChanged.AddListener(OnNicknameInputFieldValueChanged);
        roomNameInputField.onValueChanged.AddListener(OnRoomNameInputFieldValueChanged);

        createRoomButton.onClick.AddListener(OnCreateRoomButtonClick);
        confirmRoomButton.onClick.AddListener(OnConfirmRoomButtonClick);
        refreshButton.onClick.AddListener(OnRefreshButtonClick);
        leaveButton.onClick.AddListener(OnLeaveButtonClick);
        readyButton.onClick.AddListener(OnReadyButtonClick);

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

        var customProperties = new Hashtable();
        customProperties["ready"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

        roomElements.ForEach(re => Destroy(re.gameObject));
        roomElements.Clear();

        playerElements.ForEach(pe => Destroy(pe.gameObject));
        playerElements.Clear();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Room joined");

        roomListingPanel.SetActive(false);
        playerListingPanel.SetActive(true);
        createRoomModal.SetActive(false);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        PhotonNetwork.PlayerList.ToList().ForEach(p =>
        {
            var playerElement = Instantiate(playerElementPrefab, playersContainer);
            playerElement.Configure(p);

            playerElements.Add(playerElement);
        });

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
            else
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
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"Player {targetPlayer.NickName} properties updated");

        var playerElement = playerElements.SingleOrDefault(pe => pe.Player.ActorNumber == targetPlayer.ActorNumber);
        if (playerElement != null)
        {
            playerElement.Refresh(targetPlayer);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master client switched to {newMasterClient.NickName}");

        var playerElement = playerElements.SingleOrDefault(pe => pe.Player.ActorNumber == newMasterClient.ActorNumber);
        if (playerElement != null)
        {
            playerElement.Refresh(newMasterClient);
        }
    }

    #endregion

    #region Private methods

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
        if (string.IsNullOrEmpty(nicknameInputField.text))
        {
            return;
        }

        roomListingPanel.SetActive(true);
        playerListingPanel.SetActive(false);
        createRoomModal.SetActive(true);

        roomNameInputField.text = $"Stanza di {nicknameInputField.text}";
    }

    private void OnConfirmRoomButtonClick()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text) || string.IsNullOrEmpty(nicknameInputField.text))
        {
            return;
        }

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
        if (string.IsNullOrEmpty(nicknameInputField.text))
        {
            return;
        }

        PlayerPrefs.SetString("nickname", nicknameInputField.text);

        PhotonNetwork.NickName = nicknameInputField.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    private void OnReadyButtonClick()
    {
        var customProperties = new Hashtable();
        customProperties["ready"] = true;

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    private void OnLeaveButtonClick()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}
