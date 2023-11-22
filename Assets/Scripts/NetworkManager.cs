using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks {

    private readonly string gameVersion = "1";

    public InputField NickNameInput;
    public Button joinButton;
    public Text connectionInfoText;
    public bool connect;
    public bool notNameNull;

    public void Start() {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        joinButton.interactable = false;
        connectionInfoText.text = "서버와 접속중...";

        NickNameInput.text = PlayerPrefs.GetString("nickname", null);

        if (NickNameInput.text.Trim() == null || NickNameInput.text.Trim() == "") {
            notNameNull = false;
        }
        else {
            notNameNull = true;
        }
    }

    public override void OnConnectedToMaster() {
        connect = true;
        connectionInfoText.text = "온라인 : 서버에 접속했습니다.";
        nameChange();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        joinButton.interactable = false;
        connectionInfoText.text = "오프라인 : 서버 접속에 실패했습니다. " + cause.ToString();

        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect() {
        PhotonNetwork.NickName = NickNameInput.text;

        joinButton.interactable = false;

        if (PhotonNetwork.IsConnected) {
            connectionInfoText.text = "상대방을 찾는 중";
            PhotonNetwork.JoinRandomRoom();
        } else {
            connectionInfoText.text = "오프라인 : 서버 접속에 실패했습니다.";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        PhotonNetwork.CreateRoom(roomName: null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom() {
        connectionInfoText.text = "게임 접속중...";
        PhotonNetwork.LoadLevel(1);
    }

    public void nameChange() {
        if (NickNameInput.text.Trim() == null || NickNameInput.text.Trim() == "") {
            notNameNull = false;
        } else {
            notNameNull = true;
            PlayerPrefs.SetString("nickname", NickNameInput.text);
        }

        if (notNameNull && connect) {
            joinButton.interactable = true;
        } else {
            joinButton.interactable = false;
        }
    }
}
