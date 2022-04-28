using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {
    // movement related constants
    private float _speed = 0.05f;

    // collision related constants
    private int _collisionPrecision = 10;
    private float _collisionSensitivity = 1f;
    private float _magicRatio = 1 + Mathf.Sqrt(5);
    private float _drawLength = 1f;

    // irrespective of frames
    void FixedUpdate() {
        Vector3 primaryDirection = PrimaryDirection();
        DrawColliders(transform.position, primaryDirection);
        Vector3 direction = closeVectors(primaryDirection).First(vector => !IsColliding(vector));
        direction *= _speed;
        transform.up = direction;
        transform.position += direction;
    }
    Vector3 PrimaryDirection() {
        return transform.up;
    }

    bool IsColliding(Vector3 direction) {
        Debug.DrawLine(transform.position, transform.position + direction * _collisionSensitivity, Color.blue);
        return Physics.Raycast(transform.position, direction, _collisionSensitivity);
    }

    IEnumerable<Vector3> closeVectors(Vector3 vector) {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, vector);
        foreach (int num in Enumerable.Range(0, _collisionPrecision)) {
            float alpha = 2 * Mathf.Asin(Mathf.Sqrt((float)num / _collisionPrecision));
            float beta = num * _magicRatio * Mathf.PI;
            Vector3 vec = rotation * new Vector3(Mathf.Sin(alpha) * Mathf.Cos(beta), Mathf.Sin(alpha) * Mathf.Sin(beta), Mathf.Cos(alpha));
            yield return vec;
        }
    }

    void DrawColliders(Vector3 position, Vector3 direction) {
        foreach (Vector3 vec in closeVectors(direction)) {
            if(IsColliding(vec)) {
                Debug.DrawLine(position, position + vec * _drawLength, Color.red);
            } else {
                Debug.DrawLine(position, position + vec * _drawLength, Color.green);
            }
        }
    }
}
