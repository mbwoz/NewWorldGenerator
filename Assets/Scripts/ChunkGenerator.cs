using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour {

    private Dictionary<Vector3Int, GameObject> activeCubes = new Dictionary<Vector3Int, GameObject>();

    private void Update() {
        Generate();
    }

    private void Generate() {
        RemoveDistantChunks();
        AddNearChunks();
    }

    private void RemoveDistantChunks() {
        Vector3Int currentChunk = GetCurrentChunk();
        List<Vector3Int> chunksToRemove = new List<Vector3Int>();
        
        foreach (Vector3Int chunk in activeCubes.Keys) {
            if (LayerOf(chunk, currentChunk) > 1) {
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
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dz = -1; dz <= 1; dz++) {
                    Vector3Int pos = new Vector3Int(
                        currentChunk.x + dx,
                        currentChunk.y + dy,
                        currentChunk.z + dz);

                    if (!activeCubes.ContainsKey(pos)) {
                        GameObject cube = new GameObject("cube");
                        CubicGenerator cubic = cube.AddComponent(typeof(CubicGenerator)) as CubicGenerator;
                        cubic.trans = pos * CubicGenerator.gSize;
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
                pos[i] -= CubicGenerator.gSize;  // otherwise (-0.1, -0.2, -0.3) would get the same chunk as (+0.1. +0.2, +0.3)
        return new Vector3Int(
            (int)(pos[0] / CubicGenerator.gSize),
            (int)(pos[1] / CubicGenerator.gSize),
            (int)(pos[2] / CubicGenerator.gSize));
    }

    private int LayerOf(Vector3Int a, Vector3Int src) {
        return Mathf.Max(
            Mathf.Abs(a.x - src.x),
            Mathf.Abs(a.y - src.y),
            Mathf.Abs(a.z - src.z));
    }
}
