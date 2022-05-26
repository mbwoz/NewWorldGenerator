using System;
using UnityEngine;

namespace ChunkGenerate {
    public class DiamondGenerator : MonoBehaviour {

        public ComputeShader surroundShader;
        
        private ComputeBuffer maskBuffer;
        private int size = 16;
        private int[] masks;
        private int kernelIndex;

        private void Awake() {
            masks = new int[size * size * size];
            kernelIndex = surroundShader.FindKernel("FullSurround");
        }

        private void OnEnable() {
            maskBuffer = new ComputeBuffer(size * size * size, sizeof(int));
        }

        private void OnDisable() {
            maskBuffer.Release();
        }

        public void Generate(Vector3 pos) {
            pos *= size;
            surroundShader.SetInt("size", size);
            surroundShader.SetVector("transition", pos);
            surroundShader.SetBuffer(kernelIndex, "surroundings", maskBuffer);
            surroundShader.Dispatch(kernelIndex, 4, 4, 4);
            
            maskBuffer.GetData(masks, 0, 0, size * size * size);

            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    for (int k = 0; k < size; k++) {
                        if (masks[i + size * j + size * size * k] == 0 && i % 8 == 0 && j % 8 == 0 && k % 8 == 0) {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.GetComponent<Collider>().enabled = false;
                            cube.transform.position = pos + new Vector3(i, j, k) + new Vector3(0.5f, 0.5f, 0.5f);
                            //Graphics.DrawMesh(mesh, Matrix4x4.TRS(pos + new Vector3(i, j, k), Quaternion.identity, new Vector3(0.2f, 0.2f, 0.2f)), material, 0);
                        }
                    }
                }
            }
        }
        
    }
}