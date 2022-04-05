using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour {

    private readonly int numLayers = 1;

    public ComputeShader perlinComputeShader;
    private ComputeBuffer permBuffer;

    private Dictionary<Vector3Int, GameObject> activeCubes = new Dictionary<Vector3Int, GameObject>();

    private void Start() {
        int kernelIndex = perlinComputeShader.FindKernel("CSPerlin");
        permBuffer = new ComputeBuffer(Perlin.permutation.Length, sizeof(int));
        permBuffer.SetData(Perlin.permutation);
        perlinComputeShader.SetBuffer(kernelIndex, "perm", permBuffer);
    }

    private void Update() {
        RemoveDistantChunks();
        AddNearChunks();
    }

    private void OnDestroy() {
        permBuffer.Release();
    }

    private void RemoveDistantChunks() {
        Vector3Int currentChunk = GetCurrentChunk();
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

    private void AddNearChunks() {
        Vector3Int currentChunk = GetCurrentChunk();
        for (int dx = -numLayers; dx <= numLayers; dx++) {
            for (int dy = -numLayers; dy <= numLayers; dy++) {
                for (int dz = -numLayers; dz <= numLayers; dz++) {
                    Vector3Int pos = new Vector3Int(
                        currentChunk.x + dx,
                        currentChunk.y + dy,
                        currentChunk.z + dz);

                    if (!activeCubes.ContainsKey(pos)) {
                        GameObject cube = new GameObject("cube");
                        CubicGenerator cubic = cube.AddComponent(typeof(CubicGenerator)) as CubicGenerator;
                        cubic.trans = pos * CubicGenerator.size;
                        cubic.perlinComputeShader = perlinComputeShader;
                        cubic.Generate();
                        activeCubes.Add(pos, cube);
                    }
                }
            }
        }
    }

    private Vector3Int GetCurrentChunk() {
        Vector3 position = transform.position;
        float[] pos = new float[3] {position.x, position.y, position.z};
        for (int i = 0; i < 3; i++)
            if (pos[i] < 0f)
                pos[i] -= CubicGenerator.size;  // otherwise (-0.1, -0.2, -0.3) would get the same chunk as (+0.1. +0.2, +0.3)
        return new Vector3Int(
            (int)(pos[0] / CubicGenerator.size),
            (int)(pos[1] / CubicGenerator.size),
            (int)(pos[2] / CubicGenerator.size));
    }

    private int LayerOf(Vector3Int a, Vector3Int src) {
        return Mathf.Max(
            Mathf.Abs(a.x - src.x),
            Mathf.Abs(a.y - src.y),
            Mathf.Abs(a.z - src.z));
    }
}
