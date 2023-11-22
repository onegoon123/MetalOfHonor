using UnityEngine;

public class MouseCtrl : MonoBehaviour {

    public Transform playerBody;
    public float mouseSensitvity;
    public float joystickSensivity;
    public TouchCtrl touchCtrl;
    public Joystick joystick;
    public bool KeyMa;
    float yRotation = 0f;
    public bool CtrlStop;
    public float smooth;
    bool IsMine;
    public bool scope;
    Quaternion targetRotaion;
    Quaternion lookAt;
    // Start is called before the first frame update
    void Awake() {
        touchCtrl = FindObjectOfType<TouchCtrl>();
        joystick = FindObjectOfType<FixedJoystick>();

    }

    private void Start() {
        IsMine = playerBody.GetComponent<PlayerCtrl>().photonView.IsMine;
    }

    // Update is called once per frame
    void Update() {
        if (!IsMine) return;

        float mouseX;
        float mouseY;
        if (CtrlStop) {
            mouseX = 0;
            mouseY = 0;
        }
        else if (KeyMa) {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitvity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity;
        }
        else {
            if (joystick.nowTouch) {
                mouseX = joystick.Horizontal * joystickSensivity;
                mouseY = joystick.Vertical * joystickSensivity;
            }
            else {
                mouseX = touchCtrl.TouchDist.x * mouseSensitvity;
                mouseY = touchCtrl.TouchDist.y * mouseSensitvity;
            }
        }
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(yRotation * (scope ? 0.5f : 1f), 0f, 0f); ; //상하 회전
        playerBody.Rotate(new Vector3(0f, mouseX * (scope ? 0.5f : 1f), 0f));

    }

}
