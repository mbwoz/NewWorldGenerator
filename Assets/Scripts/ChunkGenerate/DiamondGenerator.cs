using System;
using System.Linq;
using UnityEngine;

namespace ChunkGenerate {
    public class DiamondGenerator : MonoBehaviour {

        public ComputeShader surroundShader;
        public GameObject diamond;
        
        private ComputeBuffer maskBuffer;
        private int size = 16;
        private int[] masks;
        private int kernelIndex;

        struct FlatWall {
            public int mask;
            public Vector3 forward, up;

            public FlatWall(int m, Vector3 u, Vector3 f) {
                mask = m;
                forward = f;
                up = u;
            }
        }

        private FlatWall[] flatSurfMasks;

        private void Awake() {
            masks = new int[size * size * size];
            kernelIndex = surroundShader.FindKernel("FullSurround");
            flatSurfMasks = new[] {
                new FlatWall(Convert.ToInt32("11110000", 2), Vector3.back, Vector3.up),
                new FlatWall(Convert.ToInt32("00001111", 2), Vector3.forward, Vector3.down),
                new FlatWall(Convert.ToInt32("10101010", 2), Vector3.left, Vector3.forward),
                new FlatWall(Convert.ToInt32("01010101", 2), Vector3.right, Vector3.forward),
                new FlatWall(Convert.ToInt32("11001100", 2), Vector3.down, Vector3.back),
                new FlatWall(Convert.ToInt32("00110011", 2), Vector3.up, Vector3.forward),
            };
        }

        private void OnEnable() {
            maskBuffer = new ComputeBuffer(size * size * size, sizeof(int));
        }

        private void OnDisable() {
            maskBuffer.Release();
        }

        public void Generate(GameObject parent, Vector3 pos) {
            pos *= size;
            surroundShader.SetInt("size", size);
            surroundShader.SetVector("transition", pos);
            surroundShader.SetBuffer(kernelIndex, "surroundings", maskBuffer);
            surroundShader.Dispatch(kernelIndex, 4, 4, 4);
            
            maskBuffer.GetData(masks, 0, 0, size * size * size);

            for (int i = 0; i < size; i += 8) {
                for (int j = 0; j < size; j += 8) {
                    for (int k = 0; k < size; k += 8) {
                        int idx = i + j * size + k * size * size;
                        for (int l = 0; l < 6; l++) {
                            if (masks[idx] == flatSurfMasks[l].mask) {
                                
                                    GameObject diam = Instantiate(
                                        diamond,
                                        pos + new Vector3(i, j, k) + new Vector3(0.5f, 0.5f, 0.5f),
                                        Quaternion.LookRotation(flatSurfMasks[l].forward, flatSurfMasks[l].up),
                                        parent.transform
                                    );
                            }
                        }
                    }
                }
            }
        }
        
    }
}