using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    private Mesh mesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public void SetUp() {
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

        meshRenderer.material.shader = Shader.Find("Diffuse");
        meshRenderer.material.color = Color.yellow;
    }

    public void UpdateMesh(ref Vector3[] vertices, ref int[] triangles) {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // force collider update
        meshCollider.enabled = false;
        meshCollider.enabled = true;
    }
}
