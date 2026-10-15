using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("--- DANH SÁCH 3 CAMERA ---")]
    public Camera[] cameras;

    private int currentCameraIndex = 0;

    private void Start()
    {
        // Kích hoạt camera đầu tiên, tắt các camera còn lại khi bắt đầu
        UpdateCameraActivation();
    }

    private void Update()
    {
        bool isTabPressed = false;

        // Nhận diện phím TAB
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isTabPressed = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTabPressed = true;
        }
#endif

        if (isTabPressed)
        {
            SwitchToNextCamera();
        }
    }

    public void SwitchToNextCamera()
    {
        if (cameras == null || cameras.Length == 0) return;

        // Chuyển xoay vòng: Cam 0 -> Cam 1 -> Cam 2 -> Cam 0...
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        UpdateCameraActivation();

        Debug.Log($">>> ĐÃ CHUYỂN SANG CAMERA: {cameras[currentCameraIndex].name} <<<");
    }

    private void UpdateCameraActivation()
    {
        if (cameras == null) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                // Chỉ bật Camera hiện tại
                cameras[i].gameObject.SetActive(i == currentCameraIndex);
            }
        }
    }
}