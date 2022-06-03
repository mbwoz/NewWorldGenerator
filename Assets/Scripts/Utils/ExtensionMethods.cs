using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace ExtensionMethods {
    public static class MyExtensions {
        private static float _magicRatio = 1 + Mathf.Sqrt(5);

        public static IEnumerable<Vector3> CloseVectors(this Vector3 vector, int count) {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, vector);
            foreach (int num in Enumerable.Range(0, count)) {
                float alpha = 2 * Mathf.Asin(Mathf.Sqrt((float)num / count));
                float beta = num * _magicRatio * Mathf.PI;
                Vector3 vec = rotation * new Vector3(Mathf.Sin(alpha) * Mathf.Cos(beta), Mathf.Sin(alpha) * Mathf.Sin(beta), Mathf.Cos(alpha));
                yield return vec;
            }
        }

        public static Vector3Int ToCubePosition(this Vector3 vector, float boxSize) {
            vector /= boxSize;
            return Vector3Int.FloorToInt(vector);
        }

        public static IEnumerable<int> Scan(this IEnumerable<int> seq) {
            int sum = 0;
            yield return sum;
            foreach (int elem in seq) {
                sum += elem;
                yield return sum;
            }
        }

        public static IEnumerable<Vector3Int> SurroundingVectors(this Vector3Int vec) {
            // 3x3x3 cube without corners. corners don't matter that much and decrease performance
            yield return vec;
//             yield return vec + Vector3Int.back;
//             yield return vec + Vector3Int.forward;
//             yield return vec + Vector3Int.left;
//             yield return vec + Vector3Int.right;
//             yield return vec + Vector3Int.down;
//             yield return vec + Vector3Int.up;
        }
    }
}
