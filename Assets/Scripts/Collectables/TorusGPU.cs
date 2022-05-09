using System;
using UnityEngine;
using UnityEngine.Rendering;

public class TorusGPU : MonoBehaviour {
    private const int maxRes = 800;
    
    [SerializeField, Range(10, maxRes)] private int resolution = 10;
    [SerializeField] private ComputeShader compShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;

    private readonly int
        posID = Shader.PropertyToID("_Positions"),
        resID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time"),
        srcPosID = Shader.PropertyToID("_SrcPos");
    
    private int numPts;
    private ComputeBuffer posBuf;
    private void Awake() {
        compShader = Instantiate(compShader);
        numPts = resolution * resolution;
    }

    private void OnEnable() {
        posBuf = new ComputeBuffer(numPts, 3 * sizeof(float));
    }

    private void OnDisable() {
        posBuf.Release();
        posBuf = null;
    }

    private void RunGPUKernel() {
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
        
        Bounds bounds = new Bounds(transform.position, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, posBuf.count, receiveShadows:false, castShadows:ShadowCastingMode.Off, lightProbeUsage:LightProbeUsage.Off);
    }

    private void Update() {
        RunGPUKernel();
    }
}