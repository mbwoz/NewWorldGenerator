using System;
using UnityEngine;

public class Graph : MonoBehaviour {
    [SerializeField] private Transform pointPrefab;
    [SerializeField, Range(10, 200)] private int resolution = 10;

    private Transform[] points;
    private int numPts;
    
    private void Awake() {
        numPts = resolution * resolution;
        points = new Transform[numPts];
        
        for (int i = 0; i < numPts; i++) {
            points[i] = Instantiate(pointPrefab);
            points[i].localScale = 2f * Vector3.one / resolution;
            points[i].SetParent(transform, true);
        }
    }

    private void Update() {
        for (int i = 0, x = 0, z = 0; i < numPts; i++, x++) {
            if (x == resolution) {
                x = 0;
                z += 1;
            }
            float t = Time.time;
            float u = (2f * (x + 0.5f) / resolution - 1f);
            float v = (2f * (z + 0.5f) / resolution - 1f);
            points[i].localPosition = WaveFunctions.Torus(u, v, t);
        }
    }
}
