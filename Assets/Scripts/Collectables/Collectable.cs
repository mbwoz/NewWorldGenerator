using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {

    private GameObject playerObj;

    private int distance = 5;
    private int range = 1;
    private int numThreads = 16;

    private float sphereRadius = 0.5f;
    private float colliderRadius = 1f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private SphereCollider sphereCollider;

    private ComputeShader surroundCS;
    private int kernelIndex;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer surroundingsBuffer;


    void Awake() { 
        positionsBuffer = new ComputeBuffer(numThreads, sizeof(int) * 3);
        surroundingsBuffer = new ComputeBuffer(numThreads, sizeof(int));
    }

    void OnDisable() {
        surroundingsBuffer.Release();
        positionsBuffer.Release();
    }

    public void SetUp(ComputeShader _surroundCS, GameObject prefab) {
        surroundCS = _surroundCS;

        kernelIndex = surroundCS.FindKernel("Surround");
        playerObj = GameObject.Find("Capsule");

        sphereCollider = gameObject.GetComponent<SphereCollider>();

        if (sphereCollider == null)
            sphereCollider = gameObject.AddComponent<SphereCollider>();

        GameObject body = Instantiate(prefab);
        body.transform.position = transform.position;
        body.transform.SetParent(transform);
        
        transform.localScale = Vector3.one * sphereRadius;
        sphereCollider.radius = colliderRadius;
        sphereCollider.isTrigger = true;

        Relocate();
    }

    private void OnTriggerEnter(Collider other) {
        CollectablesManager.UpdateScore();
        Relocate();
    }

    private void Relocate() {
        Vector3Int[] positions = new Vector3Int[numThreads];
        int[] surroundings = new int[numThreads];
        bool rerun = true;
        
        do {
            for (int i = 0; i < positions.Length; i++) {
                positions[i] = Vector3Int.RoundToInt(
                    playerObj.transform.position + Random.onUnitSphere * (distance + Random.value * range)
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
