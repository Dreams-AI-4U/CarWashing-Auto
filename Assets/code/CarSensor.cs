using UnityEngine;

public class CarSensor : MonoBehaviour
{
    public CarWashController washController;

    private void OnTriggerEnter(Collider other)
    {
        // Nhận diện xe vào vạch
        if (other.name.Contains("Car") || other.CompareTag("Player"))
        {
            // 1. Tắt script CarMove để xe dừng lại
            CarMove moveScript = other.GetComponent<CarMove>();
            if (moveScript != null)
            {
                moveScript.enabled = false;
            }

            // 2. Triệt tiêu quán tính vật lý
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. Đứng im chờ lệnh từ Bảng điều khiển (không tự chạy nữa)
            Debug.Log(">>> XE ĐÃ ĐẾN VỊ TRÍ BẢO DƯỠNG! ĐANG ĐỜI LỆNH BẤM NÚT... <<<");
        }
    }
}