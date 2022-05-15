using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ExtensionMethods;

namespace ExtensionMethods {
    public static class MyExtensions {
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
            yield return vec + Vector3Int.back;
            yield return vec + Vector3Int.forward;
            yield return vec + Vector3Int.left;
            yield return vec + Vector3Int.right;
            yield return vec + Vector3Int.down;
            yield return vec + Vector3Int.up;
        }
    }
}

public class BoidManager : MonoBehaviour {
    // grouping related constants
    private float _closeRadius = 3f;
    private float _viewRadius = 20f;

    private float _repulsion = 1.5f;
    private float _attraction = 1.5f;

    private float _groupingWeight = 1f;
    private float _directionWeight = 3f;

    // performance related constants
    public float BoxSize { get; private set; } = 50f;

    // I can't believe I actually have to add this...
    private class Vector3Comparator : IComparer<Vector3Int> {
        public int Compare(Vector3Int v1, Vector3Int v2) {
            if (v1.x > v2.x) return 1;
            if (v1.x == v2.x && v1.y > v2.y) return 1;
            if (v1.x == v2.x && v1.y == v2.y && v1.z > v2.z) return 1;
            if (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z) return 0;
            return -1;
        }
    }

    private SortedDictionary<Vector3Int, HashSet<Boid>> BoidsDict = new SortedDictionary<Vector3Int, HashSet<Boid>>(new Vector3Comparator());

    [SerializeField] private ComputeShader analyzeCS;
    private readonly int cubesPositionsID = Shader.PropertyToID("_CubesPositions");
    private readonly int cubesDirectionsID = Shader.PropertyToID("_CubesDirections");
    private readonly int boidsRangesID = Shader.PropertyToID("_BoidsRanges");
    private readonly int cubesRangesID = Shader.PropertyToID("_CubesRanges");
    private readonly int resultsID = Shader.PropertyToID("_Results");

    // how boids indices are split into cubes
    private ComputeBuffer boidsRangesBuffer;
    // positions that have to be considered in every cube 
    private ComputeBuffer cubesPositionsBuffer;
    // directions that have to be considered in every cube
    private ComputeBuffer cubesDirectionsBuffer;
    // ranges how above values are split into cubes
    private ComputeBuffer cubesRangesBuffer;

    // output, result in the order boids appear in the dictionary
    private ComputeBuffer boidsDirectionsBuffer;

    private Vector3[] boidsDirectionArray;

    private void Awake() {
        analyzeCS = Instantiate(analyzeCS);
        analyzeCS.SetFloat("_closeRadius", _closeRadius);
        analyzeCS.SetFloat("_viewRadius", _viewRadius);
        analyzeCS.SetFloat("_repulsion", _repulsion);
        analyzeCS.SetFloat("_attraction", _attraction);
        analyzeCS.SetFloat("_groupingWeight", _groupingWeight);
        analyzeCS.SetFloat("_directionWeight", _directionWeight);
    }

    private void FixedUpdate() {
        RemoveOldCubes();
        int boidsCount = BoidsDict.Sum(x => x.Value.Count);
        if (boidsCount == 0) {
            return;
        }
//         Debug.Log(boidsCount);
        RunGPUKernel();
    }

    public void AddBoid(Vector3Int cube, Boid boid) {
        if (!BoidsDict.ContainsKey(cube)) BoidsDict.Add(cube, new HashSet<Boid>());
        BoidsDict[cube].Add(boid);
    }

    public void MoveBoid(Vector3Int prev, Vector3Int next, Boid boid) {
        if (!BoidsDict.ContainsKey(next)) BoidsDict.Add(next, new HashSet<Boid>());
        BoidsDict[prev].Remove(boid);
        BoidsDict[next].Add(boid);
    }

    private void RemoveOldCubes() {
        BoidsDict.Keys.Where(key => BoidsDict[key].Count == 0).ToList().ForEach(key => BoidsDict.Remove(key));
    }

    private IEnumerable<Boid> GetBoidsAroundCube(Vector3Int cube) {
        return cube.SurroundingVectors().Where(vec => BoidsDict.ContainsKey(vec)).Aggregate(Enumerable.Empty<Boid>(), (boidList, elem) => boidList.Concat(BoidsDict.TryGetValue(cube, out var boids) ? boids : new HashSet<Boid>()));
    }

    private void RunGPUKernel() {
        int boidsCount = BoidsDict.Sum(x => x.Value.Count);
        int[] boidPosRanges = BoidsDict.Aggregate(Enumerable.Empty<int>(), (rangeList, elem) => rangeList.Append(elem.Value.Count), rangeList => rangeList.Scan().ToArray());
        Vector3[] cubesPositionsArr = BoidsDict.Aggregate(Enumerable.Empty<Vector3>(), (neighList, elem) => neighList.Concat(GetBoidsAroundCube(elem.Key).Select(boid => boid.transform.position)), neighList => neighList.ToArray());
        Vector3[] cubesDirectionsArr = BoidsDict.Aggregate(Enumerable.Empty<Vector3>(), (neighList, elem) => neighList.Concat(GetBoidsAroundCube(elem.Key).Select(boid => boid.transform.up)), neighList => neighList.ToArray());
        int[] cubesRanges = BoidsDict.Aggregate(Enumerable.Empty<int>(), (rangeList, elem) => rangeList.Append(GetBoidsAroundCube(elem.Key).Count()), rangeList => rangeList.Scan().ToArray());

        boidsRangesBuffer = new ComputeBuffer(BoidsDict.Count + 1, sizeof(int));
        boidsRangesBuffer.SetData(boidPosRanges);

        cubesPositionsBuffer = new ComputeBuffer(cubesPositionsArr.Length, sizeof(float) * 3);
        cubesPositionsBuffer.SetData(cubesPositionsArr);

        cubesDirectionsBuffer = new ComputeBuffer(cubesDirectionsArr.Length, sizeof(float) * 3);
        cubesDirectionsBuffer.SetData(cubesDirectionsArr);

        cubesRangesBuffer = new ComputeBuffer(BoidsDict.Count + 1, sizeof(int));
        cubesRangesBuffer.SetData(cubesRanges);

        boidsDirectionsBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 3);

        analyzeCS.SetBuffer(0, boidsRangesID, boidsRangesBuffer);
        analyzeCS.SetBuffer(0, cubesPositionsID, cubesPositionsBuffer);
        analyzeCS.SetBuffer(0, cubesDirectionsID, cubesDirectionsBuffer);
        analyzeCS.SetBuffer(0, cubesRangesID, cubesRangesBuffer);
        analyzeCS.SetBuffer(0, resultsID, boidsDirectionsBuffer);
        analyzeCS.Dispatch(0, BoidsDict.Count, 1, 1);

        //  collect results and distribute to boids
        boidsDirectionArray = new Vector3[boidsCount];
        boidsDirectionsBuffer.GetData(boidsDirectionArray, 0, 0, boidsCount);
        int i = 0;
        foreach (var boidsSet in BoidsDict.Values) {
            foreach (var boid in boidsSet) {
                boid.friendsDirection = boidsDirectionArray[i++];
            }
        }

        boidsRangesBuffer.Release();
        cubesPositionsBuffer.Release();
        cubesDirectionsBuffer.Release();
        cubesRangesBuffer.Release();
        boidsDirectionsBuffer.Release();
    }

}
