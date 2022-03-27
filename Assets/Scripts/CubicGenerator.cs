using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicGenerator : MonoBehaviour {
    public enum Side { Inside, Outside }

    private Mesh mesh;
    private Side[,,] dots;
    private Vector3 trans = new Vector3(0, 0, 0);

    private readonly double dotsScale = 3.0;
    private readonly double dotsSep = 0.5;

    public static readonly int gSize = 2;
    public static readonly int size = 20;

    public void Generate() {
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
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void GenerateDots() {
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

    void GenerateCubes() {
        float unit = (float)gSize / size;
        Vector3[] vertices = new Vector3[(size + 1) * (size + 1) * (size + 1)];
        for (int it = 0, y = 0; y <= size; y++) {
            for (int z = 0; z <= size; z++) {
                for (int x = 0; x <= size; x++) {
                    vertices[it] = new Vector3(x, y, z) * unit + trans;
                    it++;
                }
            }
        }

        List<Vector3Int> tris = new List<Vector3Int>();
        for (int y = 0; y < size; y++) {
            for (int z = 0; z < size; z++) {
                for (int x = 0; x < size; x++) {
                    if (dots[x + 1, y + 1, z + 1] == Side.Outside)
                        continue;

                    if (dots[x, y + 1, z + 1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y, z), new Vector3Int(x, y, z+1), new Vector3Int(x, y+1, z+1),
                            new Vector3Int(x, y+1, z+1), new Vector3Int(x, y+1, z), new Vector3Int(x, y, z)
                        });
                    }
                    if (dots[x + 2, y + 1, z + 1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x+1, y, z+1), new Vector3Int(x+1, y, z), new Vector3Int(x+1, y+1, z),
                            new Vector3Int(x+1, y+1, z), new Vector3Int(x+1, y+1, z+1), new Vector3Int(x+1, y, z+1)
                        });
                    }
                    if (dots[x + 1, y, z + 1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y, z), new Vector3Int(x+1, y, z), new Vector3Int(x+1, y, z+1),
                            new Vector3Int(x+1, y, z+1), new Vector3Int(x, y, z+1), new Vector3Int(x, y, z)
                        });
                    }
                    if (dots[x + 1, y + 2, z + 1] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x, y+1, z+1), new Vector3Int(x+1, y+1, z+1), new Vector3Int(x+1, y+1, z),
                            new Vector3Int(x+1, y+1, z), new Vector3Int(x, y+1, z), new Vector3Int(x, y+1, z+1)
                        });
                    }
                    if (dots[x + 1, y + 1, z] == Side.Outside) {
                        tris.AddRange(new Vector3Int[] {
                            new Vector3Int(x+1, y, z), new Vector3Int(x, y, z), new Vector3Int(x, y+1, z),
                            new Vector3Int(x, y+1, z), new Vector3Int(x+1, y+1, z), new Vector3Int(x+1, y, z),
                        });
                    }
                    if (dots[x + 1, y + 1, z + 2] == Side.Outside) {
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

    public void SetTrans(int x, int y, int z) {
        trans = new Vector3(x, y, z);
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireMesh(mesh);
    }
}
