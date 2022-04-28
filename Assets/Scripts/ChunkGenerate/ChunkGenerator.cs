using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour {

    struct Triangle {
        public Vector3 a, b, c;
    };

    private readonly int numLayers = 2;
    private readonly int size = 16;
    private readonly int numThreads = 8;

    public Material materialRef;
    public ComputeShader cubeCS;
    private int kernelIndex;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer trianglesCntBuffer;
    private ComputeBuffer triangulationBuffer;

    private Dictionary<Vector3Int, GameObject> activeCubes = new Dictionary<Vector3Int, GameObject>();

    private void Awake() {
        int numTriangles = size * size * size * 5;
        trianglesBuffer = new ComputeBuffer(numTriangles, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        trianglesCntBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        triangulationBuffer = new ComputeBuffer(256 * 16, sizeof(int));

        kernelIndex = cubeCS.FindKernel("Cube");
        cubeCS.SetInt("size", size);
        triangulationBuffer.SetData(MarchTable.triangulation);
        cubeCS.SetBuffer(kernelIndex, "triangulation", triangulationBuffer);
    }

    private void Update() {
        Vector3Int currentChunk = GetCurrentChunk();
        RemoveDistantChunks(currentChunk);
        AddNearChunks(currentChunk);
    }

    private void OnDisable() {
        triangulationBuffer.Release();
        trianglesCntBuffer.Release();
        trianglesBuffer.Release();
    }

    private Vector3Int GetCurrentChunk() {
        Vector3 position = transform.position;
        float[] pos = new float[3] {position.x, position.y, position.z};
        for (int i = 0; i < 3; i++) {
            if (pos[i] < 0f)
                pos[i] -= size;  // otherwise (-0.1, -0.2, -0.3) would get the same chunk as (+0.1. +0.2, +0.3)
        }

        return new Vector3Int(
            (int)(pos[0] / size),
            (int)(pos[1] / size),
            (int)(pos[2] / size)
        );
    }

    private void AddNearChunks(Vector3Int currentChunk) {
        for (int dx = -numLayers; dx <= numLayers; dx++) {
            for (int dy = -numLayers; dy <= numLayers; dy++) {
                for (int dz = -numLayers; dz <= numLayers; dz++) {
                    Vector3Int pos = new Vector3Int(
                        currentChunk.x + dx,
                        currentChunk.y + dy,
                        currentChunk.z + dz
                    );

                    if (!activeCubes.ContainsKey(pos))
                        AddChunk(pos);
                }
            }
        }
    }

    private void AddChunk(Vector3Int position) {
        GameObject gameObj = new GameObject("Chunk");
        Chunk chunk = gameObj.AddComponent(typeof(Chunk)) as Chunk;
        chunk.SetUp(materialRef);

        trianglesBuffer.SetCounterValue(0);
        cubeCS.SetBuffer(kernelIndex, "triangles", trianglesBuffer);
        cubeCS.SetVector("transition", (Vector3)position * size);

        int dispatchSize = Mathf.CeilToInt(size / numThreads);
        cubeCS.Dispatch(kernelIndex, dispatchSize, dispatchSize, dispatchSize);

        ComputeBuffer.CopyCount(trianglesBuffer, trianglesCntBuffer, 0);
        int[] trianglesCountArr = { 0 };
        trianglesCntBuffer.GetData(trianglesCountArr);
        int numTriangles = trianglesCountArr[0];

        Triangle[] tris = new Triangle[numTriangles];
        trianglesBuffer.GetData(tris, 0, 0, numTriangles);

        Vector3[] vertices = new Vector3[3 * numTriangles];
        for (int i = 0; i < numTriangles; i++) {
            vertices[3 * i] = tris[i].a;
            vertices[3 * i + 1] = tris[i].b;
            vertices[3 * i + 2] = tris[i].c;
        }

        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            triangles[i] = i;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3) {
            Vector3 normal = Vector3.Cross(
                vertices[i + 1] - vertices[i],
                vertices[i + 2] - vertices[i]
            ).normalized;

            Vector3 vDir = new Vector3(0, 0, 1);
            if (Mathf.Abs(normal.y) < 0.99f)
                vDir = (new Vector3(0, 1, 0) - normal.y * normal).normalized;
            Vector3 uDir = Vector3.Cross(normal, vDir).normalized;

            for (int j = 0; j < 3; j++) {
                uvs[i + j] = new Vector2(
                    Vector3.Dot(vertices[i + j], uDir),
                    Vector3.Dot(vertices[i + j], vDir)
                );
            }
        }

        chunk.UpdateMesh(ref vertices, ref triangles, ref uvs);
        activeCubes.Add(position, gameObj);
    }

    private void RemoveDistantChunks(Vector3Int currentChunk) {
        List<Vector3Int> chunksToRemove = new List<Vector3Int>();

        foreach (Vector3Int chunk in activeCubes.Keys) {
            if (LayerOf(chunk, currentChunk) > numLayers) {
                chunksToRemove.Add(chunk);
            }
        }
        foreach (Vector3Int chunk in chunksToRemove) {
            Destroy(activeCubes[chunk]);
            activeCubes.Remove(chunk);
        }
    }

    private int LayerOf(Vector3Int a, Vector3Int src) {
        return Mathf.Max(
            Mathf.Abs(a.x - src.x),
            Mathf.Abs(a.y - src.y),
            Mathf.Abs(a.z - src.z)
        );
    }
}
