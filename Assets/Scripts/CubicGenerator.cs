using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicGenerator : MonoBehaviour {

    public enum Side { Inside, Outside }

    public static readonly int gSize = 2;
    public static readonly int size = 20;
    public Vector3 trans { get; set; } = new Vector3(0, 0, 0);

    private Mesh mesh;

    private Side[,,] dots;
    private readonly double dotsScale = 3.0;
    private readonly double dotsSep = 0.5;

    public void Generate() {
        mesh = new Mesh();

        GenerateDots();
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

    private void GenerateDots() {
        dots = new Side[size + 2, size + 2, size + 2];
        for (int y = -1; y <= size; y++) {
            for (int z = -1; z <= size; z++) {
                for (int x = -1; x <= size; x++) {
                    double unit = ((double)gSize / size);
                    double coorX = (((double)x + 0.5) * unit + trans.x) * dotsScale;
                    double coorY = (((double)y + 0.5) * unit + trans.y) * dotsScale;
                    double coorZ = (((double)z + 0.5) * unit + trans.z) * dotsScale;

                    dots[x + 1, y + 1, z + 1] 
                        = (Perlin.perlin(coorX, coorY, coorZ) < dotsSep)
                        ? Side.Inside : Side.Outside;

                    // closed cube for debugging only
                    // if (x == -1 || y == -1 || z == -1 || x == size || y == size || z == size) {
                    //     dots[x+1, y+1, z+1] = Side.Outside;
                    // }
                }
            }
        }
    }

    private void GenerateCubes() {
        List<Vector3> vertices = new List<Vector3>();

        for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    if (dots[x + 1, y + 1, z + 1] == Side.Outside)
                        continue;

                    if (dots[x, y + 1, z + 1] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z), new Vector3(x, y, z+1),
                            new Vector3(x, y+1, z+1), new Vector3(x, y+1, z)
                        });
                    }
                    if (dots[x + 2, y + 1, z + 1] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x+1, y, z+1), new Vector3(x+1, y, z),
                            new Vector3(x+1, y+1, z), new Vector3(x+1, y+1, z+1)
                        });
                    }
                    if (dots[x + 1, y, z + 1] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z), new Vector3(x+1, y, z),
                            new Vector3(x+1, y, z+1), new Vector3(x, y, z+1)
                        });
                    }
                    if (dots[x + 1, y + 2, z + 1] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y+1, z+1), new Vector3(x+1, y+1, z+1),
                            new Vector3(x+1, y+1, z), new Vector3(x, y+1, z)
                        });
                    }
                    if (dots[x + 1, y + 1, z] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x+1, y, z), new Vector3(x, y, z),
                            new Vector3(x, y+1, z), new Vector3(x+1, y+1, z)
                        });
                    }
                    if (dots[x + 1, y + 1, z + 2] == Side.Outside) {
                        vertices.AddRange(new Vector3[] {
                            new Vector3(x, y, z+1), new Vector3(x+1, y, z+1),
                            new Vector3(x+1, y+1, z+1), new Vector3(x, y+1, z+1)
                        });
                    }
                }
            }
        }

        float unit = (float)gSize / size;
        for (int it = 0; it < vertices.Count; it++) {
            vertices[it] = vertices[it] * unit + trans;
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
