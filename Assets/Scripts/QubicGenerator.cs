using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Side {
    Inside = 0,
    Outside = 1
}

public class QubicGenerator : MonoBehaviour {
    private Mesh mesh;

    private Side[,,] dots;

    private int gSize = 2;
    private int size = 10;

    void Start() {
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        GetComponent<MeshFilter>().mesh = mesh;

        GenerateDots();
        GenerateCubes();
    }

    void GenerateDots() {
        dots = new Side[size, size, size];
        for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    dots[x, y, z] = (Random.value < .5f) ? Side.Inside : Side.Outside;
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
                            new Vector3Int(x, y, z), new Vector3Int(x, y+1, z), new Vector3Int(x+1, y+1, z),
                            new Vector3Int(x+1, y+1, z), new Vector3Int(x+1, y, z), 
                            new Vector3Int(x, y, z)
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
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.black;
        Gizmos.DrawWireMesh(mesh);
    }
}