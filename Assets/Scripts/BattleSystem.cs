using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public enum State {
    Select, Ready
}

public class BattleSystem : MonoBehaviourPun {

    public PlayerCtrl[] Players;
    public State[] States;
    public GameObject selectUI;
    public Text text;
    public Text playerScoreText;
    public Text enemyScoreText;
    int playerScore;
    int enemyScore;
    public GameObject[] selects;
    public GameObject touchUI;
    public float selectTime;
    public float battleTime;
    bool keyma;
    int i;

    [PunRPC]
    public void GameStartRPC() {
        StartCoroutine(StartDelay());
    }

    IEnumerator StartDelay() {
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.75f);
        text.text = "2";
        yield return new WaitForSeconds(0.75f);
        text.text = "1";
        yield return new WaitForSeconds(0.75f);
        text.gameObject.SetActive(false);

        keyma = GameManager.Instance.player.KeyMa;
        
        Players = FindObjectsOfType<PlayerCtrl>();
        States = new State[2];
        if (Players[0].Equals(GameManager.Instance.player)) {
            GameManager.Instance.enemy = Players[1].transform;
        }
        else {
            GameManager.Instance.enemy = Players[0].transform;
        }
        GameManager.Instance.enemySet();
        SelectTime();
    }
    public void SelectTime() {
        CheckPlayer();

        GameManager.Instance.player.photonView.RPC("AllStopRPC", RpcTarget.All);

        selects[0].SetActive(true);
        selects[1].SetActive(true);
        selects[2].SetActive(false);
        selectUI.SetActive(true);

        if (!keyma) touchUI.SetActive(false);
        if (keyma) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (playerScore == 5 || enemyScore == 5) {
            return;
        }
        else {
            StopAllCoroutines();
            StartCoroutine(SelectTime(selectTime));
        }
    }

    void StartCombat() {
        CheckPlayer();

        GameManager.Instance.player.photonView.RPC("AllStartRPC", RpcTarget.All);
        selectUI.SetActive(false);
        if (!keyma) touchUI.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(findEnemyTime());
        if (keyma) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    [PunRPC]
    public void PickRPC(int playerNum) {
        States[playerNum] = State.Ready;

        if (ReadyCheck()) {
            StartCombat();
        }
    }

    bool ReadyCheck() {
        bool check = true;
        foreach(State state in States) {
            if (state != State.Ready)
                check = false;
        }
        return check;
    }

    void CheckPlayer() {
        if(PhotonNetwork.CurrentRoom.PlayerCount < 2) {
            GameManager.Instance.LeftRoom();
        }
    }

    IEnumerator SelectTime(float t) {
        yield return new WaitForSeconds(t);
        GameManager.Instance.player.gunCtrl.photonView.RPC("SelectMainRPC", RpcTarget.All, GameManager.Instance.player.gunCtrl.main);
        GameManager.Instance.player.gunCtrl.photonView.RPC("SelectServeRPC", RpcTarget.All, GameManager.Instance.player.gunCtrl.serve);
        GameManager.Instance.player.gunCtrl.photonView.RPC("SwapMainRPC", RpcTarget.All);
        photonView.RPC("PickRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }
    IEnumerator findEnemyTime() {
        yield return new WaitForSeconds(5);
        CheckPlayer();
        yield return StartCoroutine(findEnemyTime());
    }

    public void PlayerPoint() {
        playerScoreText.text = (++playerScore).ToString();
        if (playerScore == 5) {
            gameOver();
        }
    }
    public void EnemyPoint() {
        enemyScoreText.text = (++enemyScore).ToString();
        if (enemyScore == 5) {
            gameOver();
        }
    }

    public void gameOver() {
        if (!keyma) touchUI.SetActive(false);
        selectUI.SetActive(false);
        if (playerScore>enemyScore) text.text = "승리";
        else if (enemyScore>playerScore) text.text = "패배";
        else text.text = "오류";
        text.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(WaitEnd());
    }

    public void stopCorutine() {
        StopAllCoroutines();
    }
    IEnumerator WaitEnd() {
        yield return new WaitForSeconds(3f);
        GameManager.Instance.LeftRoom();
    }
}
