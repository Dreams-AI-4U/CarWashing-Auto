using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class mainha : MonoBehaviour
{
    public float bottomWidth = 6f; // Chiều rộng đáy dưới
    public float topWidth = 4f;    // Chiều rộng đáy trên
    public float height = 2f;      // Chiều cao mái
    public float length = 10f;     // Chiều dài mái

    private void OnEnable()
    {
        CreateTrapezoid();
    }

    private void OnValidate()
    {
        CreateTrapezoid();
    }

    void CreateTrapezoid()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) return;

        Mesh mesh = new Mesh();
        float halfB = bottomWidth / 2f;
        float halfT = topWidth / 2f;
        float halfL = length / 2f;

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfB, 0, -halfL),
            new Vector3(halfB, 0, -halfL),
            new Vector3(-halfT, height, -halfL),
            new Vector3(halfT, height, -halfL),

            new Vector3(-halfB, 0, halfL),
            new Vector3(halfB, 0, halfL),
            new Vector3(-halfT, height, halfL),
            new Vector3(halfT, height, halfL)
        };

        int[] triangles = new int[]
        {
            // Mặt trước
            0, 2, 1, 2, 3, 1,
            // Mặt sau
            5, 7, 4, 7, 6, 4,
            // Mái trái
            0, 4, 2, 2, 4, 6,
            // Mái phải
            1, 3, 5, 3, 7, 5,
            // Mặt trên
            2, 6, 3, 3, 6, 7
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;
    }
}