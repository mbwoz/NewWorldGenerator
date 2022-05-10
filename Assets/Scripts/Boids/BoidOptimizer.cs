using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidOptimizer : MonoBehaviour
{
    private Dictionary<Vector3, HashSet<Boid>> BoidsDict = new Dictionary<Vector3, HashSet<Boid>>();

    private void EnsureKey(Vector3 key) {
        if (!BoidsDict.ContainsKey(key)) {
            BoidsDict.Add(key, new HashSet<Boid>());
        }
    }

    public HashSet<Boid> GetBoidsInCube(Vector3 cube) {
        EnsureKey(cube);
        return BoidsDict[cube];
    }

    public void AddBoid(Vector3 cube, Boid boid) {
        EnsureKey(cube);
        BoidsDict[cube].Add(boid);
    }

    public void MoveBoid(Vector3 prev, Vector3 next, Boid boid) {
        EnsureKey(next);
        BoidsDict[prev].Remove(boid);
        BoidsDict[next].Add(boid);
    }

}
