using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private float _speed = 1f;
    private float _collisionSensitivity = 5f;
    private float _scale;
    // Start is called before the first frame update
    void Start()
    {
        _scale = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // irrespective of frames
    void FixedUpdate()
    {
        Vector3 primaryDirection = PrimaryDirection();
        if (HeadingForCollision(primaryDirection))
        {
            primaryDirection = AvoidCollision(primaryDirection);
        }
        Vector3 movement = Vector3.ClampMagnitude(primaryDirection, _speed);
        transform.position += movement;
    }
    Vector3 PrimaryDirection()
    {
        return transform.up;
    }
    
    bool HeadingForCollision(Vector3 direction)
    {
        Debug.Log(transform.position);
        return Physics.Raycast(transform.position, transform.up, _scale/2 + _collisionSensitivity);
    }
    
    Vector3 AvoidCollision(Vector3 direction)
    {
        return Vector3.zero;
    }
    
}
