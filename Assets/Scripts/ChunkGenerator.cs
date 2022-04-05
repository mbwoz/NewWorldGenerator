using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour {

    struct Square {
        public Vector3 a, b, c, d;
    };

    private readonly int numLayers = 3;
    private readonly int size = 16;
    private readonly int numThreads = 8;

    public ComputeShader cubeCS;
    private int kernelIndex;
    private ComputeBuffer squaresBuffer;
    private ComputeBuffer squaresCntBuffer;

    private Dictionary<Vector3Int, GameObject> activeCubes = new Dictionary<Vector3Int, GameObject>();

    private void Awake() {
        int numSquares = size * size * size * 3;
        squaresBuffer = new ComputeBuffer(numSquares, sizeof(float) * 3 * 4, ComputeBufferType.Append);
        squaresCntBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        kernelIndex = cubeCS.FindKernel("Cube");
        cubeCS.SetInt("size", size);
    }

    private void Update() {
        Vector3Int currentChunk = GetCurrentChunk();
        RemoveDistantChunks(currentChunk);
        AddNearChunks(currentChunk);
    }

    private void OnDisable() {
        squaresCntBuffer.Release();
        squaresBuffer.Release();
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

        squaresBuffer.SetCounterValue(0);
        cubeCS.SetBuffer(kernelIndex, "squares", squaresBuffer);
        cubeCS.SetVector("transition", (Vector3)position * size);

        int dispatchSize = Mathf.CeilToInt(size / numThreads);
        cubeCS.Dispatch(kernelIndex, dispatchSize, dispatchSize, dispatchSize);

        ComputeBuffer.CopyCount(squaresBuffer, squaresCntBuffer, 0);
        int[] squaresCountArr = { 0 };
        squaresCntBuffer.GetData(squaresCountArr);
        int numSquares = squaresCountArr[0];

        Square[] squares = new Square[numSquares];
        squaresBuffer.GetData(squares, 0, 0, numSquares);

        Vector3[] vertices = new Vector3[4 * numSquares];
        int[] triangles = new int[6 * numSquares];

        for (int i = 0; i < numSquares; i++) {
            vertices[4 * i] = squares[i].a;
            vertices[4 * i + 1] = squares[i].b;
            vertices[4 * i + 2] = squares[i].c;
            vertices[4 * i + 3] = squares[i].d;
        }

        for (int i = 0; i < numSquares; i++) {
            triangles[6 * i] = 4 * i;
            triangles[6 * i + 1] = 4 * i + 1;
            triangles[6 * i + 2] = 4 * i + 2;
            triangles[6 * i + 3] = 4 * i + 2;
            triangles[6 * i + 4] = 4 * i + 3;
            triangles[6 * i + 5] = 4 * i;
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
