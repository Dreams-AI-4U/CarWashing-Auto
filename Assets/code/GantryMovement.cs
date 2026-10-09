using UnityEngine;

public class GantryMovement : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 1.5f;       // Tốc độ di chuyển
    public Transform startPoint;          // Điểm bắt đầu (đầu thanh ray)
    public Transform endPoint;            // Điểm kết thúc (cuối thanh ray)

    private Vector3 targetPosition;

    void Start()
    {
        // Ban đầu hướng di chuyển sẽ về phía endPoint
        if (endPoint != null)
        {
            targetPosition = endPoint.position;
        }
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        // Tự động di chuyển Gantry về phía targetPosition
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Khi đến điểm đích, tự đổi hướng quay lại điểm kia
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            if (targetPosition == endPoint.position)
            {
                targetPosition = startPoint.position;
            }
            else
            {
                targetPosition = endPoint.position;
            }
        }
    }
}