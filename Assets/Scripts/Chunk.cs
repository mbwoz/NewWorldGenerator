using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    }

    public void UpdateMesh(ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uvs) {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        // force collider update
        meshCollider.enabled = false;
        meshCollider.enabled = true;
        meshRenderer.material = material;
    }
}
