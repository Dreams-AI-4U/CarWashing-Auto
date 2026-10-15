using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("--- CẤU HÌNH TRƯỢT CỬA ---")]
    [Tooltip("Khoảng cách trượt (Ví dụ: 1.2 hoặc -1.2)")]
    public float slideDistance = 1.2f;

    [Tooltip("Trục trượt Local (Mặc định Vector3.right)")]
    public Vector3 slideAxis = Vector3.right;

    [Tooltip("Tốc độ đóng/mở cửa")]
    public float slideSpeed = 2.5f;

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private bool isOpen = false;
    private Coroutine moveCoroutine;

    private void Start()
    {
        // Lưu vị trí ban đầu
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + (slideAxis.normalized * slideDistance);
    }

    private void Update()
    {
        bool isClicked = false;
        Vector2 mousePos = Vector2.zero;

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
            // Tự động tìm Camera đang BẬT trong Scene khi bấm Tab
            Camera activeCam = Camera.main;
            if (activeCam == null || !activeCam.isActiveAndEnabled)
            {
                Camera[] allCams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in allCams)
                {
                    if (cam.isActiveAndEnabled)
                    {
                        activeCam = cam;
                        break;
                    }
                }
            }

            if (activeCam == null) return;

            Ray ray = activeCam.ScreenPointToRay(mousePos);
            RaycastHit[] hits = Physics.RaycastAll(ray, 500f);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    ToggleDoor();
                    break;
                }
            }
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        Vector3 targetPos = isOpen ? openLocalPos : closedLocalPos;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveDoorRoutine(targetPos));
    }

    private IEnumerator MoveDoorRoutine(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, slideSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = target;
    }
}