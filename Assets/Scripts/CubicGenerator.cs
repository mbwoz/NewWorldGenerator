using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicGenerator : MonoBehaviour {
    public enum Side { Inside, Outside }

    private Mesh mesh;

    private Side[,,] dots;
    private readonly double dotsScale = 3.0;
    private readonly double dotsSep = 0.5;

    private readonly int gSize = 2;
    private readonly int size = 20;

    void Start() {
        mesh = new Mesh();

        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material.shader = Shader.Find("Diffuse");
        GetComponent<MeshRenderer>().material.color = Color.yellow;

        GenerateDots();
        GenerateCubes();
    }

    void GenerateDots() {
        dots = new Side[size, size, size];
        for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    double scale = ((double)gSize / size) * dotsScale;
                    double coorX = ((double)x + 0.5) * scale;
                    double coorY = ((double)y + 0.5) * scale;
                    double coorZ = ((double)z + 0.5) * scale;

                    dots[x, y, z] = (Perlin.perlin(coorX, coorY, coorZ) < dotsSep)
                        ? Side.Inside : Side.Outside;
                }
            }
        }
    }

    void GenerateCubes() {
        float unit = (float)gSize / size;
        Vector3[] vertices = new Vector3[(size + 1) * (size + 1) * (size + 1)];
        for (int it = 0, y = 0; y <= size; y++) {
            for (int z = 0; z <= size; z++) {
                for (int x = 0; x <= size; x++) {
                    vertices[it] = new Vector3(x, y, z) * unit;
                    it++;
                }
            }
        }

        List<Vector3Int> tris = new List<Vector3Int>();
        for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    if (dots[x, y, z] == Side.Outside)
                        continue;

                    if (x - 1 < 0 || dots[x-1, y, z] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y, z), new Vector3Int(x, y, z+1), new Vector3Int(x, y+1, z+1),
                            new Vector3Int(x, y+1, z+1), new Vector3Int(x, y+1, z), new Vector3Int(x, y, z)
                        });
                    }
                    if (x + 1 >= size || dots[x+1, y, z] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x+1, y, z+1), new Vector3Int(x+1, y, z), new Vector3Int(x+1, y+1, z),
                            new Vector3Int(x+1, y+1, z), new Vector3Int(x+1, y+1, z+1), new Vector3Int(x+1, y, z+1)
                        });
                    }
                    if (y - 1 < 0 || dots[x, y-1, z] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y, z), new Vector3Int(x+1, y, z), new Vector3Int(x+1, y, z+1),
                            new Vector3Int(x+1, y, z+1), new Vector3Int(x, y, z+1), new Vector3Int(x, y, z)
                        });
                    }
                    if (y + 1 >= size || dots[x, y+1, z] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y+1, z+1), new Vector3Int(x+1, y+1, z+1), new Vector3Int(x+1, y+1, z),
                            new Vector3Int(x+1, y+1, z), new Vector3Int(x, y+1, z), new Vector3Int(x, y+1, z+1)
                        });
                    }
                    if (z - 1 < 0 || dots[x, y, z-1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x+1, y, z), new Vector3Int(x, y, z), new Vector3Int(x, y+1, z),
                            new Vector3Int(x, y+1, z), new Vector3Int(x+1, y+1, z), new Vector3Int(x+1, y, z),
                        });
                    }
                    if (z + 1 >= size || dots[x, y, z+1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y, z+1), new Vector3Int(x+1, y, z+1), new Vector3Int(x+1, y+1, z+1),
                            new Vector3Int(x+1, y+1, z+1), new Vector3Int(x, y+1, z+1), new Vector3Int(x, y, z+1)
                        });
                    }
                }
            }
        }
        int[] triangles = new int[tris.Count];
        for (int it = 0; it < tris.Count; it++) {
            triangles[it] = tris[it].x + (size + 1) * tris[it].z + (size + 1) * (size + 1) * tris[it].y;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireMesh(mesh);
    }
}