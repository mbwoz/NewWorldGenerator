using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    private float _minSpawnRadius = 1f;
    private float _maxSpawnRadius = 2f;
    private int _boidsPerClick = 100;
    public GameObject boid;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            for (int i = 0; i < _boidsPerClick; i++) {
                SpawnBoid();
            }
        }
    }
    
    void SpawnBoid() {
        Vector3 position = transform.position + Random.insideUnitSphere * Random.Range(_minSpawnRadius, _maxSpawnRadius);
        Quaternion direction = Random.rotation;
        Instantiate(boid, position, direction);
    }
}
