using UnityEngine;
using Photon.Pun;

public class PlayerCtrl : MonoBehaviourPun {

    public bool IsMasterClientLocal => PhotonNetwork.IsMasterClient && photonView.IsMine;

    public CharacterController controller;
    public Animator firstAnimator;
    public Animator thirdAnimator;

    public float speed;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Material loaclColor;
    public Material enemyColor;
    public SkinnedMeshRenderer mesh;

    public GunCtrl gunCtrl;
    public MouseCtrl mouseCtrl;
    public Joystick joystick;

    Vector3 velocity;
    bool isGrounded;
    bool local;
    public bool crouch;

    public bool CtrlStop;
    public bool KeyMa;
    public bool JumpKey;
    public GameObject WalkSound;
    bool stopSound;
    float soundWait;
    public Collider[] colliders;

    private void Awake() {
        if (photonView.IsMine) {
            mesh.material = loaclColor;
            transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(0).gameObject.GetComponent<MouseCtrl>().KeyMa = KeyMa;
            foreach (Collider collider in colliders) {
                collider.enabled = false;
            }
        } else {
            mesh.material = enemyColor;
            transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(5).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(0).gameObject.GetComponent<AudioListener>().enabled = false;
            Destroy(transform.GetChild(1).gameObject.GetComponent<MouseCtrl>());
        }

        if (KeyMa) {
            Destroy(GameObject.FindWithTag("TouchUI"));
            gunCtrl.keyma = true;
        }
        joystick = FindObjectOfType<FloatingJoystick>();
        
    }

    void Update() {
        if (!photonView.IsMine) return;
        if (stopSound) {
            soundWait += Time.deltaTime;
            if (soundWait >= 0.6) {
                stopSound = false;
                soundWait = 0;
            }
        }
        Movement();
        Crouch();
    }

    void Movement() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        firstAnimator.SetBool("Jump", !isGrounded);
        thirdAnimator.SetBool("Jump", !isGrounded);

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        float x = 0;
        float z = 0;

        if (!CtrlStop) {
            if (KeyMa) {
                x = Input.GetAxis("Horizontal");
                z = Input.GetAxis("Vertical");
                JumpKey = Input.GetButtonDown("Jump");
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl)) crouch = !crouch;
            }
            else {
                x = joystick.Horizontal;
                z = joystick.Vertical;
            }

            if (JumpKey && isGrounded) {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                firstAnimator.SetBool("Jump", true);
                thirdAnimator.SetBool("Jump", true);
            }
        }

        firstAnimator.SetFloat("Direction", x);
        firstAnimator.SetFloat("Speed", z);
        thirdAnimator.SetFloat("Direction", x);
        thirdAnimator.SetFloat("Speed", z);

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Mathf.Abs(x) + Mathf.Abs(z) > 0.5 && !crouch && !stopSound && isGrounded) {
            stopSound = true;
            photonView.RPC("SoundRPC", RpcTarget.All);
        }
    }
    
    void Crouch() {
        if(crouch && isGrounded) {
            firstAnimator.SetBool("Crouch", true);
            thirdAnimator.SetBool("Crouch", true);
            speed = 1.5f;
        } else {
            firstAnimator.SetBool("Crouch", false);
            thirdAnimator.SetBool("Crouch", false);
            speed = 3.2f;
        }
    }

    [PunRPC]
    public void AllStopRPC() {
        if (!KeyMa) {
            joystick.OnPointerUp(null);
            gunCtrl.joystick.OnPointerUp(null);
        }
        CtrlStop = true;
        gunCtrl.CtrlStop = true;
        mouseCtrl.CtrlStop = true;
        gunCtrl.swapWait = true;
        firstAnimator.SetBool("Attack", false);
        thirdAnimator.SetBool("Shoot", false);
        crouch = false;
        JumpKey = false;
        gunCtrl.recoil = 0;
        gunCtrl.lookScope = false;
        gunCtrl.mouse.scope = false;
    }

    [PunRPC]
    public void AllStartRPC() {
        CtrlStop = false;
        gunCtrl.CtrlStop = false;
        mouseCtrl.CtrlStop = false;
    }

    [PunRPC]
    public void SoundRPC() {
        Instantiate(WalkSound, transform.position, Quaternion.identity);
    }
}
