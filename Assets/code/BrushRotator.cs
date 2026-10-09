using UnityEngine;

public class BrushRotator : MonoBehaviour
{
    public float speed = 300f; // Toc do xoay
    public Vector3 rotationAxis = Vector3.up; // Trục xoay (Y)

    void Update()
    {
        // Xoay lien tuc quanh truc duoc chi dinh
        transform.Rotate(rotationAxis * speed * Time.deltaTime);
    }
}