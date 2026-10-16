using UnityEngine;

public class ControlButton3D : MonoBehaviour
{
    [Header("--- CẤU HÌNH NÚT BẤM ---")]
    public int stepNumber = 1;
    public CarWashController washController;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        if (washController == null)
        {
            washController = FindFirstObjectByType<CarWashController>();
        }
    }

    private void Update()
    {
        bool isClicked = false;
        Vector2 mousePos = Vector2.zero;

        // Bắt sự kiện click chuột tương thích cả New Input System lẫn Old Input
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            isClicked = true;
            mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            isClicked = true;
            mousePos = Input.mousePosition;
        }
#endif

        if (isClicked)
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Ray ray = mainCam.ScreenPointToRay(mousePos);
            RaycastHit[] hits = Physics.RaycastAll(ray, 500f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Debug.Log($"[BẢNG ĐIỀU KHIỂN] Kích hoạt thành công Nút {stepNumber}!");
                    
                    if (washController != null)
                    {
                        washController.ExecuteStep(stepNumber);
                    }
                    else
                    {
                        Debug.LogError("Chưa gán CarWashController!");
                    }
                    break;
                }
            }
        }
    }
}