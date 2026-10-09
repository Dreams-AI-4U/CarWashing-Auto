using UnityEngine;

public class CarSensor : MonoBehaviour
{
    public CarWashController washController;

    private void OnTriggerEnter(Collider other)
    {
        // Nhan dien xe
        if (other.name.Contains("Car") || other.CompareTag("Player"))
        {
            // 1. Tat han script CarMove de xe dung di chuyen
            CarMove moveScript = other.GetComponent<CarMove>();
            if (moveScript != null)
            {
                moveScript.enabled = false;
            }

            // 2. Xoa sach quan tinh vat ly
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. Goi may rua xe hoat dong
            if (washController != null)
            {
                washController.StartWashProcess();
            }
            else
            {
                Debug.LogError("Chua gan _GameController vao o Wash Controller cua CarStopSensor!");
            }
        }
    }
}