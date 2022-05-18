using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    private float _minSpawnRadius = 1f;
    private float _maxSpawnRadius = 2f;
    private int _boidsPerClick = 100;
    private BoidManager manager;
    // Start is called before the first frame update
    void Start() {
        manager = (BoidManager) FindObjectOfType(typeof(BoidManager));
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            for (int i = 0; i < _boidsPerClick; i++) {
                manager.NewBoid(transform.position + Random.insideUnitSphere * Random.Range(_minSpawnRadius, _maxSpawnRadius));
            }
        }
    }
}
