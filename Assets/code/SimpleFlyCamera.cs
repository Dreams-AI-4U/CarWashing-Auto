using UnityEngine;

public class SimpleFlyCamera : MonoBehaviour
{
    [Header("--- CẤU HÌNH TỐC ĐỘ CAMERA ---")]
    public float speed = 8.0f;                  // Tốc độ di chuyển mặc định
    public float fastSpeedMultiplier = 3.0f;    // Hệ số nhân tốc độ khi giữ Shift (Gấp 3 lần)
    public float sensitivity = 2.0f;            // Tốc độ xoay chuột

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Tự động lấy góc xoay hiện tại của Camera làm mốc ban đầu (không can thiệp vị trí)
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void Update()
    {
        float currentSpeed = speed;

        // GIỮ CHUỘT PHẢI ĐỂ XOAY GÓC NHÌN
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
            rotationY += mouseDelta.x * sensitivity * 0.1f;
            rotationX -= mouseDelta.y * sensitivity * 0.1f;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        // DI CHUYỂN BẰNG WASD + QE (E: Bay lên, Q: Bay xuống)
        Vector3 move = Vector3.zero;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var kbd = UnityEngine.InputSystem.Keyboard.current;

            // KHI GIỮ SHIFT: TĂNG TỐC CAMERA
            if (kbd.leftShiftKey.isPressed || kbd.rightShiftKey.isPressed)
            {
                currentSpeed *= fastSpeedMultiplier;
            }

            if (kbd.wKey.isPressed) move += transform.forward;
            if (kbd.sKey.isPressed) move -= transform.forward;
            if (kbd.aKey.isPressed) move -= transform.right;
            if (kbd.dKey.isPressed) move += transform.right;
            if (kbd.eKey.isPressed) move += transform.up; // Bay lên
            if (kbd.qKey.isPressed) move -= transform.up; // Bay xuống
        }
        
        if (move != Vector3.zero)
        {
            transform.position += move.normalized * currentSpeed * Time.deltaTime;
        }
#else
        if (Input.GetMouseButton(1))
        {
            rotationY += Input.GetAxis("Mouse X") * sensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        // KHI GIỮ SHIFT: TĂNG TỐC CAMERA
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= fastSpeedMultiplier;
        }

        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveH + transform.forward * moveV;

        if (Input.GetKey(KeyCode.E)) move += transform.up;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;

        if (move != Vector3.zero)
        {
            transform.position += move.normalized * currentSpeed * Time.deltaTime;
        }
#endif
    }
}