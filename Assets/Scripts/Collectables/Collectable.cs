using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {

    private GameObject playerObj;

    private int distance = 5;
    private int numThreads = 16;

    public ComputeShader surroundCS;
    private int kernelIndex;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer surroundingsBuffer;

    void Awake() { 
        positionsBuffer = new ComputeBuffer(numThreads, sizeof(int) * 3);
        surroundingsBuffer = new ComputeBuffer(numThreads, sizeof(int));
        kernelIndex = surroundCS.FindKernel("Surround");
    }

    void Start() {
        transform.localScale = Vector3.one * 0.5f;
        playerObj = GameObject.Find("Capsule");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.N)) {
            RespawnObject();
        }
    }

    void OnDisable() {
        surroundingsBuffer.Release();
        positionsBuffer.Release();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Collected");
        RespawnObject();
    }

    private void RespawnObject() {
        Vector3Int[] positions = new Vector3Int[numThreads];
        int[] surroundings = new int[numThreads];
        bool rerun = true;
        
        do {
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = Vector3Int.RoundToInt(
                    playerObj.transform.position + Random.onUnitSphere * distance
                );
            }

            surroundCS.SetBuffer(kernelIndex, "positions", positionsBuffer);
            positionsBuffer.SetData(positions);
            surroundCS.SetBuffer(kernelIndex, "surroundings", surroundingsBuffer);

            surroundCS.Dispatch(kernelIndex, 1, 1, 1);

            surroundingsBuffer.GetData(surroundings);

            for (int i = 0; i < surroundings.Length; i++) {
                if (surroundings[i] == 0) {
                    transform.position = positions[i] + new Vector3(0.5f, 0.5f, 0.5f);
                    rerun = false;
                    break;
                }
            }
        } while (rerun);
    }
}