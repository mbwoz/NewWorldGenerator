using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour {

    struct Triangle {
        public int a, b, c;
    };

    private readonly int numLayers = 1;
    private readonly int size = 8;
    private readonly int numThreads = 8;

    private Vector3[] vertices;

    public ComputeShader cubeCS;
    private int kernelIndex;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer trianglesCntBuffer;

    private Dictionary<Vector3Int, GameObject> activeCubes = new Dictionary<Vector3Int, GameObject>();

    private void Awake() {
        int numTriangles = size * size * size * 5;
        trianglesBuffer = new ComputeBuffer(numTriangles, sizeof(int) * 3, ComputeBufferType.Append);
        trianglesCntBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        kernelIndex = cubeCS.FindKernel("Cube");
        cubeCS.SetInt("size", size);

        vertices = new Vector3[3 * (size + 1) * (size + 1) * (size + 1)];
        int it = 0;
        for (int z = 0; z <= size; z++) {
            for (int y = 0; y <= size; y++) {
                for (int x = 0; x <= size; x++) {
                    vertices[it] = new Vector3(x + 0.5f, y, z);
                    vertices[it + 1] = new Vector3(x, y + 0.5f, z);
                    vertices[it + 2] = new Vector3(x, y, z + 0.5f);
                    it += 3;
                }
            }
        }
    }

    private void Update() {
        Vector3Int currentChunk = GetCurrentChunk();
        RemoveDistantChunks(currentChunk);
        AddNearChunks(currentChunk);
    }

    private void OnDisable() {
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
        chunk.SetUp();

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

        int[] triangles = new int[3 * numTriangles];
        for (int i = 0; i < numTriangles; i++) {
            triangles[3 * i] = tris[i].a;
            triangles[3 * i + 1] = tris[i].b;
            triangles[3 * i + 2] = tris[i].c;
        }

        chunk.UpdateMesh(ref vertices, ref triangles);
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
            Mathf.Abs(a.z - src.z));
    }
}
