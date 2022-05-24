using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

    private Mesh mesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material material;

    private Vector3 position;
    private int size;

    public void SetUp(Material materialRef) {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
        }

        material = materialRef;
    }

    public void UpdateMesh(Vector3 _position, int _size) {
        if (position == _position && size == _size)
            return;

        position = _position;
        size = _size;

        Vector3[] vertices = new Vector3[24];
        vertices[0] = vertices[8] = vertices[16] = position;
        vertices[1] = vertices[11] = vertices[20] = position + Vector3.right * size;
        vertices[2] = vertices[12] = vertices[23] = position + Vector3.right * size + Vector3.up * size;
        vertices[3] = vertices[15] = vertices[17] = position + Vector3.up * size;
        vertices[4] = vertices[9] = vertices[19] = position + Vector3.forward * size;
        vertices[5] = vertices[14] = vertices[18] = position + Vector3.forward * size + Vector3.up * size;
        vertices[6] = vertices[13] = vertices[22] = position + Vector3.forward * size + Vector3.right * size + Vector3.up * size;
        vertices[7] = vertices[10] = vertices[21] = position + Vector3.forward * size + Vector3.right * size;

        int[] triangles = new int[36];
        for (int i = 0; i < 6; i++) {
            triangles[6 * i] = 4 * i;
            triangles[6 * i + 1] = 4 * i + 1;
            triangles[6 * i + 2] = 4 * i + 2;
            triangles[6 * i + 3] = 4 * i + 2;
            triangles[6 * i + 4] = 4 * i + 3;
            triangles[6 * i + 5] = 4 * i;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshRenderer.material = material;
    }
}
