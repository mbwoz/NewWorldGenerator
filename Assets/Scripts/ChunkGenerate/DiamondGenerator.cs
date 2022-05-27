using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private int hashFunc(int x, int y, int z) {
            return x + y * 13 + z * 31;
        }

        public void Generate(GameObject parent, Vector3 pos) {
            pos *= size;
            surroundShader.SetInt("size", size);
            surroundShader.SetVector("transition", pos);
            surroundShader.SetBuffer(kernelIndex, "surroundings", maskBuffer);
            surroundShader.Dispatch(kernelIndex, 2, 2, 2);
            
            maskBuffer.GetData(masks, 0, 0, size * size * size);

            Random.seed = hashFunc((int)pos.x, (int)pos.y, (int)pos.z);

            int shots = 4 * size;

            for (int i = 0; i < shots; i++) {
                int shot = Random.Range(0, size * size * size);
                for (int l = 0; l < 6; l++) {
                    if (masks[shot] == flatSurfMasks[l].mask) {
                        int x = shot % size;
                        int y = (shot / size) % size;
                        int z = (shot / size) / size;
                        GameObject diam = Instantiate(
                            diamond,
                            pos + new Vector3(x, y, z) + new Vector3(0.5f, 0.5f, 0.5f),
                            Quaternion.LookRotation(flatSurfMasks[l].forward, flatSurfMasks[l].up),
                            parent.transform
                        );
                        masks[shot] = -1;
                    }
                }
            }
        }
    }
}