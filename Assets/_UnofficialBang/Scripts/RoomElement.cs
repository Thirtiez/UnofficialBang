using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoomElement : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text playersText;

    [SerializeField]
    private Button joinButton;

    public RoomInfo RoomInfo { get; private set; }

    public void Configure(RoomInfo roomInfo, UnityAction onJoinButtonClick)
    {
        Refresh(roomInfo);

        nameText.text = roomInfo.Name;

        joinButton.onClick.AddListener(onJoinButtonClick);
    }

    public void Refresh(RoomInfo roomInfo)
    {
        RoomInfo = roomInfo;

        playersText.text = $"Giocatori: {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    public void SetInteractable(string nickname)
    {
        joinButton.interactable = RoomInfo.PlayerCount < RoomInfo.MaxPlayers && !string.IsNullOrEmpty(nickname);
    }
}
