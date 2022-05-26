using System;
using UnityEngine;
using UnityEngine.Rendering;

public class TorusGPU : MonoBehaviour {
    private const int maxRes = 800;
    
    [SerializeField, Range(10, maxRes)] private int resolution = 10;
    [SerializeField] private ComputeShader compShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;
    [SerializeField] private float renderDistance = 40f;
    
    private readonly int
        posID = Shader.PropertyToID("_Positions"),
        resID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time"),
        srcPosID = Shader.PropertyToID("_SrcPos");
    
    private int numPts;
    private ComputeBuffer posBuf;
    private Vector3[] computedPositions;
    private Vector3 meshScale;
    
    private void Awake() {
        compShader = Instantiate(compShader);
        numPts = resolution * resolution;
        computedPositions = new Vector3[numPts];
        float scale = 2f / resolution;
        meshScale = new Vector3(scale, scale, scale);
    }

    private void OnEnable() {
        posBuf = new ComputeBuffer(numPts, 3 * sizeof(float));
    }

    private void OnDisable() {
        posBuf.Release();
        posBuf = null;
    }

    public void RunGPUKernel(Collectable[] collectables, Vector3 playerPos) {
        float step = 2f / resolution;
        compShader.SetInt(resID, resolution);
        compShader.SetFloat(stepID, step);
        compShader.SetFloat(timeID, Time.time);
        compShader.SetFloats(srcPosID, transform.position.x, transform.position.y, transform.position.z);
        compShader.SetBuffer(0, posID, posBuf);
        int groups = Mathf.CeilToInt(resolution / 16f);
        material.SetBuffer(posID, posBuf);
        material.SetFloat(stepID, step);
        compShader.Dispatch(0, groups, groups, 1);
        
        posBuf.GetData(computedPositions, 0, 0, numPts);
        
        foreach (Collectable col in collectables) {
            // to save computation only render close collectables
            if (Vector3.Distance(playerPos, col.transform.position) <= renderDistance) {
                for (int i = 0; i < numPts; i++) {
                    Graphics.DrawMesh(mesh,
                        Matrix4x4.TRS(col.transform.position + computedPositions[i], Quaternion.identity, meshScale),
                        material,
                        0);
                }
            }
        }
    }
}
