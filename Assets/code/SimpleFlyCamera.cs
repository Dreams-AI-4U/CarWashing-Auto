using UnityEngine;

public class SimpleFlyCamera : MonoBehaviour
{
    public float speed = 5.0f;
    public float sensitivity = 2.0f;

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
            if (kbd.wKey.isPressed) move += transform.forward;
            if (kbd.sKey.isPressed) move -= transform.forward;
            if (kbd.aKey.isPressed) move -= transform.right;
            if (kbd.dKey.isPressed) move += transform.right;
            if (kbd.eKey.isPressed) move += transform.up; // Bay lên
            if (kbd.qKey.isPressed) move -= transform.up; // Bay xuống
        }
        
        if (move != Vector3.zero)
        {
            transform.position += move.normalized * speed * Time.deltaTime;
        }
#else
        if (Input.GetMouseButton(1))
        {
            rotationY += Input.GetAxis("Mouse X") * sensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveH + transform.forward * moveV;

        if (Input.GetKey(KeyCode.E)) move += transform.up;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;

        if (move != Vector3.zero)
        {
            transform.position += move.normalized * speed * Time.deltaTime;
        }
#endif
    }
}