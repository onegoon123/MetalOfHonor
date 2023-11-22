using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Target : MonoBehaviourPun
{
    public int MaxHP;
    public int currentHP;
    public AudioSource dead;
    bool god;
    bool useIndicator;
    public RectTransform indicator;
    Image indicatorImage;
    Vector3 incomingDir;
    public GameObject charObj;
    public GameObject BlueRagdollPrefab;
    public GameObject RedRagdollPrefab;
    GameObject ragdollObj;
    public GameObject cam;

    private void Start() {
        indicator = GameManager.Instance.indicator;
        indicatorImage = indicator.GetComponent<Image>();
    }
    private void Update() {
        if (!photonView.IsMine) return;
        if (useIndicator) {
            indicator.rotation = Quaternion.Euler(0, 0, -GetHitAngle());
        }
    }

    [PunRPC]
    public void DamageRPC(int damage) {
        if (god) return;
        currentHP -= damage;

        if (photonView.IsMine) {
            StartCoroutine(UseIndicator());
            incomingDir = GameManager.Instance.enemy.forward;
            GameManager.Instance.myBar.value = (float)currentHP / MaxHP;
        } else {
            GameManager.Instance.enemyBar.value = (float)currentHP / MaxHP;
        }

        if (currentHP <= 0) {
            GetComponent<PlayerCtrl>().AllStopRPC();
            ragdoll();
            dead.Play();
            currentHP = MaxHP;
            StartCoroutine(GodMod());
            
            
            if (photonView.IsMine) {
                cam.SetActive(true);
                GameManager.Instance.battleSystem.EnemyPoint();
            } else {
                GameManager.Instance.battleSystem.PlayerPoint();
            }
        }
    }

    IEnumerator GodMod() {
        god = true;
        yield return new WaitForSeconds(1.5f);
        Respawn();
        yield return new WaitForSeconds(0.5f);
        charObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        god = false;
    }

    void Respawn() {
        if (photonView.IsMine) {
            cam.SetActive(false);
            GameManager.Instance.battleSystem.SelectTime();
            GameManager.Instance.myBar.value = 1;
            float Max = 0;
            Transform enemy = GameManager.Instance.enemy;
            Transform spawn = null;
            foreach (Transform t in GameManager.Instance.spawnPositions) {
                float dis = Vector3.Distance(enemy.position, t.position);
                if (Max < dis) {
                    spawn = t;
                    Max = dis;
                }
            }
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        } else {
            GameManager.Instance.enemyBar.value = 1;
        }
        GetComponent<GunCtrl>().photonView.RPC("ActiveRPC", RpcTarget.All);
        
    }
    IEnumerator UseIndicator() {
        useIndicator = true;
        indicatorImage.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        float t = 1;
        while (t > 0) {
            t -= Time.deltaTime * 2;
            indicatorImage.color = new Color(1, 0, 0, t);
            yield return null;
        }
        indicatorImage.color = new Color(1, 0, 0, 0);
        useIndicator = false;
    }

    float GetHitAngle() {

        Vector3 otherDir = new Vector3(-incomingDir.x, 0f, -incomingDir.z);
        Vector3 playerFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        float angle = Vector3.SignedAngle(playerFwd, otherDir, Vector3.up);

        return angle;
    }

    void ragdoll() {
        GameObject ragD = null;
        if (photonView.IsMine) {
            ragD = Instantiate(BlueRagdollPrefab, transform.position + (Vector3.down * 0.6f), transform.rotation);
            ragD.GetComponent<DestroyLife>().deathPhysics(incomingDir.normalized);
        }
        else {
            ragD = Instantiate(RedRagdollPrefab, transform.position + (Vector3.down * 0.6f), transform.rotation);
            CopyAnimCharacterTransformToRagdoll(charObj.transform, ragD.transform);
            ragD.GetComponent<DestroyLife>().deathPhysics(GameManager.Instance.player.transform.forward);
        }
        
        charObj.SetActive(false);
    }

    private void CopyAnimCharacterTransformToRagdoll(Transform origin, Transform rag) {
        for (int i = 0; i < origin.childCount; i++) {
            
            if (origin.childCount != 0) {
                CopyAnimCharacterTransformToRagdoll(origin.GetChild(i), rag.GetChild(i));
            }
            rag.GetChild(i).localPosition = origin.GetChild(i).localPosition;
            rag.GetChild(i).localRotation = origin.GetChild(i).localRotation;
        }
    }
}
