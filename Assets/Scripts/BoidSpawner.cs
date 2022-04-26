using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    private float _minSpawnRadius = 10f;
    private float _maxSpawnRadius = 20f;
    public GameObject boid;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SpawnBoid();
    }
    
    void SpawnBoid()
    {
        Vector3 position = Random.insideUnitSphere * Random.Range(_minSpawnRadius, _maxSpawnRadius);
        Quaternion direction = Random.rotation;
        Debug.Log("Spawning boid!");
        Instantiate(boid, position, direction);
        
    }
}
