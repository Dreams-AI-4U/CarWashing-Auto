using UnityEngine;

public class CarMove : MonoBehaviour
{
    public float speed = 3.0f;

    void Update()
    {
        // Xe tu dong tien ve phia truoc theo huong mat xe
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}