using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicGenerator : MonoBehaviour {
    
    public static readonly int size = 14;

    public Vector3 trans { get; set; } = new Vector3(0, 0, 0);
    public ComputeShader perlinComputeShader { get; set; }

    private Mesh mesh;
    private int[] dots;

    public void Generate() {
        mesh = new Mesh();

        GPUDots();
        GenerateCubes();

        if (gameObject.GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (gameObject.GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();
        if (gameObject.GetComponent<MeshCollider>() == null)
            gameObject.AddComponent<MeshCollider>();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Diffuse");
        gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void GPUDots() {
        int kernelIndex = perlinComputeShader.FindKernel("CSPerlin");

        dots = new int[(size + 2) * (size + 2) * (size + 2)];
        ComputeBuffer dotsBuffer = new ComputeBuffer(dots.Length, sizeof(int));
        dotsBuffer.SetData(dots);
        perlinComputeShader.SetBuffer(kernelIndex, "dots", dotsBuffer);

        perlinComputeShader.SetInt("size", size + 2);
        perlinComputeShader.SetVector("trans", trans);

        perlinComputeShader.Dispatch(
            kernelIndex,
            Mathf.CeilToInt((float)(size + 2) / 8),
            Mathf.CeilToInt((float)(size + 2) / 8),
            size + 2);

        dotsBuffer.GetData(dots);
        dotsBuffer.Release();
    }

    private void GenerateCubes() {
        List<Vector3> vertices = new List<Vector3>();

        for (int z = 0; z < size; z++) {
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    int pos = x + 1 + (size + 2) * (y + 1 + (size + 2) * (z + 1));
                    if (dots[pos] == 1)
                        continue;

                    if (dots[pos - 1] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z), new Vector3(x, y, z+1),
                            new Vector3(x, y+1, z+1), new Vector3(x, y+1, z)
                        });
                    }
                    if (dots[pos + 1] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x+1, y, z+1), new Vector3(x+1, y, z),
                            new Vector3(x+1, y+1, z), new Vector3(x+1, y+1, z+1)
                        });
                    }
                    if (dots[pos - (size + 2)] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z), new Vector3(x+1, y, z),
                            new Vector3(x+1, y, z+1), new Vector3(x, y, z+1)
                        });
                    }
                    if (dots[pos + (size + 2)] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y+1, z+1), new Vector3(x+1, y+1, z+1),
                            new Vector3(x+1, y+1, z), new Vector3(x, y+1, z)
                        });
                    }
                    if (dots[pos - (size + 2) * (size + 2)] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x+1, y, z), new Vector3(x, y, z),
                            new Vector3(x, y+1, z), new Vector3(x+1, y+1, z)
                        });
                    }
                    if (dots[pos + (size + 2) * (size + 2)] == 1) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z+1), new Vector3(x+1, y, z+1),
                            new Vector3(x+1, y+1, z+1), new Vector3(x, y+1, z+1)
                        });
                    }
                }
            }
        }

        for (int it = 0; it < vertices.Count; it++) {
            vertices[it] += trans;
        }

        List<int> triangles = new List<int>();

        for (int it = 0; it < vertices.Count; it += 4) {
            triangles.AddRange(new int[] {
                it, it + 1, it + 2, it + 2, it + 3, it
            });
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireMesh(mesh);
    }
}
