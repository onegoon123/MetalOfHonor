using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {

    public static GameManager Instance {
        get {
            if (instance == null) instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }

    private static GameManager instance;
    public Transform[] spawnPositions;
    public GameObject playerPrefab;
    public PlayerCtrl player;
    public Transform enemy;
    public BattleSystem battleSystem;
    public Slider myBar;
    public Slider enemyBar;
    public Text myName;
    public Text enemyName;
    public Text nowBullet;
    public Text bullet;
    public RectTransform indicator;
    public bool keyma;

    private void Awake() {
        Application.targetFrameRate = 60;

    }
    void Start() {
        if (!keyma) QualitySettings.vSyncCount = 0;
        SpawnPlayer();
        myName.text = PhotonNetwork.NickName;
        photonView.RPC("nameSetRPC", RpcTarget.OthersBuffered, PhotonNetwork.NickName);
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2) {
            GameStart();

            if (keyma) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void GameStart() {
        GetComponent<BattleSystem>().photonView.RPC("GameStartRPC", RpcTarget.All);
    }

    private void SpawnPlayer() {
        int localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Debug.Log(localPlayerIndex);
        Transform spawnPosition = spawnPositions[localPlayerIndex % spawnPositions.Length];

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position, spawnPosition.rotation).GetComponent<PlayerCtrl>();
    }


    public override void OnLeftRoom() {
        PhotonNetwork.LoadLevel(0);
    }

    public void LeftRoom() {
        SceneManager.LoadScene(0);
        PhotonNetwork.LeaveRoom();
        if (keyma) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SelectMain(int num) {
        player.gunCtrl.photonView.RPC("SelectMainRPC", RpcTarget.All, num);
    }
    public void SelectServe(int num) {
        player.gunCtrl.photonView.RPC("SelectServeRPC", RpcTarget.All, num);
        player.gunCtrl.photonView.RPC("SwapMainRPC", RpcTarget.All);
        battleSystem.photonView.RPC("PickRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    public void Swap() {
        if (!player.gunCtrl.nowWeaponMain) player.gunCtrl.photonView.RPC("SwapMainRPC", RpcTarget.All);
        else player.gunCtrl.photonView.RPC("SwapServeRPC", RpcTarget.All);
    }

    bool c = false;
    public void Crouch() {
        c = !c;
        player.crouch = c;
    }
    public void Jump(bool b) {
        player.JumpKey = b;
    }

    [PunRPC]
    public void nameSetRPC(string name) {
        enemyName.text = name;
    }

    public void Reload() {
        player.gunCtrl.DoReload();
    }

    public void SetBulletUI(int now, int max) {
        nowBullet.text = now.ToString();
        bullet.text = max.ToString();
    }

    public void enemySet() {
        player.gunCtrl.SetTarget(enemy);
    }
}
