using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    private Mesh mesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Material material;

    public void SetUp(Material materialRef) {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshCollider = gameObject.GetComponent<MeshCollider>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
        }

        if (meshCollider.sharedMesh == null)
            meshCollider.sharedMesh = mesh;

        material = materialRef;
        //meshRenderer.material.shader = Shader.Find("Diffuse");
        //meshRenderer.material.color = Color.yellow;
    }

    public void UpdateMesh(ref Vector3[] vertices, ref int[] triangles) {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x + vertices[i].y, vertices[i].z + vertices[i].y);
        }
        
        /*for (int i = 0; i < triangles.Length; i += 3) {
            Vector3[] v = {vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]};
            Vector3 normal = Vector3.Cross(v[1] - v[0], v[2] - v[0]).normalized;
            for (int j = 0; j < 3; j++) {
                uvs[triangles[i + j]] = Vector3.Dot(normal, v[i]);
            }
        }*/

        mesh.uv = uvs;

        // force collider update
        meshCollider.enabled = false;
        meshCollider.enabled = true;
        meshRenderer.material = material;
    }
}
