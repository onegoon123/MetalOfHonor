using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
public enum WeaponType {
    Rifle, Sniper, Shotgun, Melee, Item
}
public class GunCtrl : MonoBehaviourPun {

    [System.Serializable]
    public struct Gun {
        public string name;
        public int damage;
        public float fireRate;
        public float range;
        public WeaponType type;
        public float swapTime;
        public int bullet;
        public int moreBullet;
        public float reloadTime;
        public float recoil;
    }
    
    public Gun[] guns;
    public int main;
    public int serve;
    public bool nowWeaponMain = true;
    public Transform[] gunTransformF;
    public Transform[] gunTransformT;
    private Transform currentGun;
    public int damage;
    public float range;
    public float fireRate;
    public WeaponType weaponType;
    public Camera fpsCam;
    public Camera gunCam;
    public MouseCtrl mouse;
    public ParticleSystem[] muzzleFlash;
    public Animator animator;
    public Animator TPSAnimator;
    public GameObject impactEffect;
    public GameObject impactEffectSmall;

    private float nextTimeToFire = 0f;

    public bool CtrlStop;
    public Joystick joystick;

    public Transform recoilMod;
    public GameObject weapon;
    public float maxRecoil_x = -20;
    public float recoilSpeed = 10;
    public float recoil = 0.0f;

    public bool lookScope;
    public bool swapWait;
    private float swapTime;
    public bool keyma;
    public GameObject[] Sound;
    public AudioSource audio;
    public Image[] hitmarker;
    int mainMaxBullet;
    int mainBullet;
    int haveMainBullet;
    int serveMaxBullet;
    int serveBullet;
    int haveServeBullet;
    float reloadT;
    float gunRecoil;
    Target enemyTarget;
    public LayerMask layer;

    private void Awake() {
        if (photonView.IsMine) {
            joystick = FindObjectOfType<FixedJoystick>();
            mouse = fpsCam.transform.GetComponent<MouseCtrl>();
            hitmarker = GameObject.FindWithTag("marker").GetComponentsInChildren<Image>();
            return;
        }

        joystick = FindObjectOfType<FixedJoystick>();
        mouse = fpsCam.transform.GetComponent<MouseCtrl>();
        
    }

    public void SetTarget(Transform enemy) {
        enemyTarget = enemy.GetComponent<Target>();
    }
    // Update is called once per frame
    void Update() {
        if (!photonView.IsMine) {
            return;
        }
        if (CtrlStop || swapWait) {
            recoiling();
            return;
        }
        if (keyma) {
            joystick.nowTouch = Input.GetMouseButton(0);

            if (Input.GetKeyDown(KeyCode.Q)) {
                if (nowWeaponMain) {
                    photonView.RPC("SwapServeRPC", RpcTarget.All);
                }
                else {
                    photonView.RPC("SwapMainRPC", RpcTarget.All);
                }
            }

            if (Input.GetMouseButton(1)) {
                mouse.scope = true;
                fpsCam.fieldOfView = 30;
                gunCam.fieldOfView = 30;
            }
            else {
                mouse.scope = false;
                fpsCam.fieldOfView = 60;
                gunCam.fieldOfView = 60;
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                DoReload();
            }
        }
        if (weaponType == WeaponType.Sniper) {
            if (keyma) Shoot();
            else Scope();
        }
        else if (weaponType == WeaponType.Rifle || weaponType == WeaponType.Shotgun) {
            Shoot();
        }
        else if (weaponType == WeaponType.Melee) {
            MeleeAttack();
        }

        recoiling();
    }
    
    [PunRPC]
    public void ActiveRPC() {
        foreach(Transform t in gunTransformF) {
            t.gameObject.SetActive(false);
        }
        foreach (Transform t in gunTransformT) {
            t.gameObject.SetActive(false);
        }
    }
    [PunRPC]
    public void SelectMainRPC(int num) {
        main = num;
        mainMaxBullet = guns[main].bullet;
        mainBullet = mainMaxBullet;
        haveMainBullet = guns[main].moreBullet;
    }
    [PunRPC]
    public void SelectServeRPC(int num) {
        serve = num;

        serveMaxBullet = guns[serve].bullet;
        serveBullet = serveMaxBullet;
        haveServeBullet = guns[serve].moreBullet;
        
    }
    [PunRPC]
    public void SwapMainRPC() {
        lookScope = false;
        nowWeaponMain = true;
        if (!photonView.IsMine) {
            gunTransformT[serve].gameObject.SetActive(false);
            currentGun = gunTransformT[main];
            currentGun.gameObject.SetActive(true);

            muzzleFlash = currentGun.GetComponentsInChildren<ParticleSystem>();
            return;
        }

        gunTransformF[serve].gameObject.SetActive(false);
        currentGun = gunTransformF[main];
        currentGun.gameObject.SetActive(true);

        muzzleFlash = currentGun.GetComponentsInChildren<ParticleSystem>();


        TPSAnimator.SetInteger("Weapon", main);
        Gun currentGunStat = guns[main];
        damage = currentGunStat.damage;
        fireRate = currentGunStat.fireRate;
        range = currentGunStat.range;
        weaponType = currentGunStat.type;
        swapTime = currentGunStat.swapTime;
        gunRecoil = currentGunStat.recoil;
        reloadT = currentGunStat.reloadTime;

        fpsCam.fieldOfView = 60;
        gunCam.fieldOfView = 60;
        GameManager.Instance.SetBulletUI(mainBullet, haveMainBullet);
        TPSAnimator.SetTrigger("Reset");
        swapWait = true;
        StartCoroutine(WaitTime(swapTime));
    }
    [PunRPC]
    public void SwapServeRPC() {
        nowWeaponMain = false;
        if (!photonView.IsMine) {
            gunTransformT[main].gameObject.SetActive(false);
            currentGun = gunTransformT[serve];
            currentGun.gameObject.SetActive(true);

            muzzleFlash = currentGun.GetComponentsInChildren<ParticleSystem>();
            return;
        }

        gunTransformF[main].gameObject.SetActive(false);
        currentGun = gunTransformF[serve];
        currentGun.gameObject.SetActive(true);

        muzzleFlash = currentGun.GetComponentsInChildren<ParticleSystem>();


        TPSAnimator.SetInteger("Weapon", serve);
        Gun currentGunStat = guns[serve];
        damage = currentGunStat.damage;
        fireRate = currentGunStat.fireRate;
        range = currentGunStat.range;
        weaponType = currentGunStat.type;
        swapTime = currentGunStat.swapTime;
        gunRecoil = currentGunStat.recoil;
        reloadT = currentGunStat.reloadTime;

        fpsCam.fieldOfView = 60;
        gunCam.fieldOfView = 60;
        GameManager.Instance.SetBulletUI(serveBullet, haveServeBullet);
        TPSAnimator.SetTrigger("Reset");
        swapWait = true;
        StartCoroutine(WaitTime(swapTime));
    }
    void Shoot() {
        if (joystick.nowTouch && Time.time >= nextTimeToFire) {
            
            nextTimeToFire = Time.time + 1f / fireRate;
            if (!useBullet()) return;
            TPSAnimator.SetBool("Shoot", true);

            foreach (ParticleSystem particleSystem in muzzleFlash) {
                particleSystem.Play();
            }
            photonView.RPC("EffectRPC", RpcTarget.All);
            photonView.RPC("SoundRPC", RpcTarget.All, 0);
            recoil += gunRecoil;

            if (weaponType == WeaponType.Shotgun) {
                RaycastHit hit;
                
                for (int i = 0; i < 20; i++) {
                    Vector3 RandomVector = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                    
                    if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + RandomVector, out hit, range, layer)) {
                        
                        GameObject impactObject = Instantiate(impactEffectSmall, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactObject, 1f);

                        if (hit.transform.CompareTag("Player")) {
                            enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                            audio.Play();
                            StartCoroutine(HitM());
                        }
                    }
                }
            }
            else {
                RaycastHit hit;

                Physics.SphereCast(fpsCam.transform.position, 0.15f, fpsCam.transform.forward, out hit, range, layer);
                if (hit.transform == null) return;
                if (hit.transform.CompareTag("Player")) {
                    enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                    audio.Play();
                    StartCoroutine(HitM());
                    GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactObject, 1f);
                }
                else if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, layer)) {
                    
                    if (hit.transform == null) return;
                    GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactObject, 1f);

                    if (hit.transform.CompareTag("Player")) {
                        enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                        audio.Play();
                        StartCoroutine(HitM());
                    }
                }
            }

        }
        else if (Time.time >= nextTimeToFire) {
            TPSAnimator.SetBool("Shoot", false);
        }
    }
    void recoiling() {
        if (recoil > 1) recoil = 1;
        if (recoil > 0) {
            Quaternion maxRecoil = Quaternion.Euler(maxRecoil_x * recoil, 0, 0);
            recoilMod.localRotation = Quaternion.Slerp(recoilMod.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);
            weapon.transform.localEulerAngles = new Vector3(recoilMod.localEulerAngles.x, weapon.transform.localEulerAngles.y, weapon.transform.localEulerAngles.z);
            recoil -= Time.deltaTime;
        }
        else {
            recoil = 0;
            Quaternion minRecoil = Quaternion.Euler(0, 0, 0);
            recoilMod.localRotation = Quaternion.Slerp(recoilMod.localRotation, minRecoil, Time.deltaTime * recoilSpeed / 2);
            weapon.transform.localEulerAngles = new Vector3(recoilMod.localEulerAngles.x, weapon.transform.localEulerAngles.y, weapon.transform.localEulerAngles.z);
        }
    }

    void Scope() {

        if (joystick.nowTouch && Time.time >= nextTimeToFire) {
            lookScope = true;
            mouse.scope = true;
            fpsCam.fieldOfView = 30;
            gunCam.fieldOfView = 30;
        } else if (lookScope) {
            if (!useBullet()) return;
            foreach (ParticleSystem particleSystem in muzzleFlash) {
                particleSystem.Play();
            }
            photonView.RPC("EffectRPC", RpcTarget.All);
            photonView.RPC("SoundRPC", RpcTarget.All, 1);
            mouse.scope = false;
            fpsCam.fieldOfView = 60;
            gunCam.fieldOfView = 60;
            TPSAnimator.SetTrigger("Shoot 0");
            RaycastHit hit;
            recoil += gunRecoil;
            Physics.SphereCast(fpsCam.transform.position, 0.15f, fpsCam.transform.forward, out hit, range, layer);
            if (hit.transform == null) return;
            if (hit.transform.CompareTag("Player")) {
                enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                audio.Play();
                StartCoroutine(HitM());
                GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObject, 1f);
            }
            else if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, layer)) {

                if (hit.transform == null) return;
                GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObject, 1f);

                if (hit.transform.CompareTag("Player")) {
                    enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                    audio.Play();
                    StartCoroutine(HitM());
                }
            }

            lookScope = false;
            nextTimeToFire = Time.time + 1f / fireRate;
        }

    }

    void MeleeAttack() {

        if (joystick.nowTouch) {
            TPSAnimator.SetBool("Shoot", true);
            animator.SetBool("Attack", true);
        } else {
            TPSAnimator.SetBool("Shoot", false);
            animator.SetBool("Attack", false);
        }
    }

    public void MAttack() {
        if (weaponType != WeaponType.Melee) return;
        photonView.RPC("SoundRPC", RpcTarget.All, 2);
        RaycastHit hit;
        if (Physics.BoxCast(fpsCam.transform.position, new Vector3(1f, 1f, 1f), fpsCam.transform.forward, out hit, transform.rotation, range, layer)) {
            if (hit.transform.CompareTag("Player")) {
                enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                audio.Play();
                StartCoroutine(HitM());
                GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactObject, 1f);
            }
        }
        else if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, layer)) {

            if (hit.transform == null) return;
            GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObject, 1f);

            if (hit.transform.CompareTag("Player")) {
                enemyTarget.photonView.RPC("DamageRPC", RpcTarget.All, damage);
                audio.Play();
                StartCoroutine(HitM());
            }
        }
    }

    [PunRPC]
    void EffectRPC() {
        if (photonView.IsMine) return;
        foreach (ParticleSystem particleSystem in muzzleFlash) {
            particleSystem.Play();
        }
    }

    IEnumerator WaitTime(float t) {
        yield return new WaitForSeconds(t);
        swapWait = false;
    }

    [PunRPC]
    public void SoundRPC(int num) {
        Instantiate(Sound[num], transform.position, Quaternion.identity);
    }

    IEnumerator HitM() {
        foreach (Image image in hitmarker) {
            image.color = Color.white;
        }
        float t = 1;
        while (t > 0) {
            t -= Time.deltaTime * 10;
            foreach (Image image in hitmarker) {
                image.color = new Color(1, 1, 1, t);
            }
            yield return null;
        }
        foreach (Image image in hitmarker) {
            image.color = new Color(1, 1, 1, 0);
        }
        yield return null;
    }

    bool useBullet() {
        if (nowWeaponMain) {
            if (mainBullet > 0) {
                mainBullet--;
                GameManager.Instance.SetBulletUI(mainBullet, haveMainBullet);
                return true;
            } else if (haveMainBullet <= 0) {
                //총알 없는 소리
                return false;
            } else {
                DoReload();
                return false;
            }
        } else {
            if (serveBullet > 0) {
                serveBullet--;
                GameManager.Instance.SetBulletUI(serveBullet, haveServeBullet);
                return true;
            } else if (haveServeBullet <= 0) {
                //총알 없는 소리
                return false;
            } else {
                DoReload();
                return false;
            }
        }
    }

    public void DoReload() {
        if (swapWait) return;
        StartCoroutine(Reload());
    }
    IEnumerator Reload() {
        
        if (nowWeaponMain) {
            if (mainBullet < mainMaxBullet && 0 < haveMainBullet) {
                lookScope = false;
                mouse.scope = false;
                fpsCam.fieldOfView = 60;
                gunCam.fieldOfView = 60;
                photonView.RPC("SoundRPC", RpcTarget.All, 3);
                swapWait = true;
                yield return new WaitForSeconds(0.01f);
                animator.SetBool("reload", true);
                yield return new WaitForSeconds(reloadT);
                swapWait = false;
                animator.SetBool("reload", false);

                //mainMaxBullet - mainBullet = 비어있는 총알 수
                if (mainMaxBullet - mainBullet >= haveMainBullet) { //비어있는 총알수 가 남아있는 총알 이상 일때
                    mainBullet += haveMainBullet;
                    haveMainBullet = 0;
                } else {
                    haveMainBullet -= mainMaxBullet - mainBullet;
                    mainBullet = mainMaxBullet;
                }
                GameManager.Instance.SetBulletUI(mainBullet, haveMainBullet);
            }
        } else {
            if (serveBullet < serveMaxBullet && 0 < haveServeBullet) {
                lookScope = false;
                mouse.scope = false;
                fpsCam.fieldOfView = 60;
                gunCam.fieldOfView = 60;
                photonView.RPC("SoundRPC", RpcTarget.All, 3);
                swapWait = true;
                animator.SetBool("reload", true);
                yield return new WaitForSeconds(reloadT);
                swapWait = false;
                animator.SetBool("reload", false);

                if (serveMaxBullet - serveBullet >= haveServeBullet) { //비어있는 총알수 가 남아있는 총알 이상 일때
                    serveBullet += haveServeBullet;
                    haveServeBullet = 0;
                }
                else {
                    haveServeBullet -= serveMaxBullet - serveBullet;
                    serveBullet = serveMaxBullet;
                }
                GameManager.Instance.SetBulletUI(serveBullet, haveServeBullet);
            }
        }
    }

}
