using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {
    // here are constants related to a single boid
    // constants related to how boids group and interact with each other are in the manager
    // movement related constants
    private float _speed = 0.1f;
    private float _stubborness = 7f;
    private float _conscientiousness = 1f;

    // collision related constants
    private int _collisionPrecision = 20;
    private float _collisionSensitivity = 1f;
    private float _magicRatio = 1 + Mathf.Sqrt(5);

    // performance related constants
    private float _boxSize;
    private BoidManager manager;
    public Vector3 friendsDirection { private get; set; }

    void Awake() {
        manager = (BoidManager) FindObjectOfType(typeof(BoidManager));
        _boxSize = manager.BoxSize;
        manager.AddBoid(PositionToCubeLocation(transform.position), this);
    }

    // irrespective of frames
    void FixedUpdate() {
        Vector3Int oldPosition = PositionToCubeLocation(transform.position);
        Vector3 destinationDirection = Vector3.zero; // TODO
        // friendsDirection calculated on GPU and already normalized
        Vector3 wantsToGo = (friendsDirection + transform.up * _stubborness + destinationDirection * _conscientiousness).normalized;

        Vector3 direction = closeVectors(wantsToGo).First(vector => !IsColliding(vector)).normalized;
        transform.up = direction;
        direction *= _speed;
        transform.position += direction;

        Vector3Int newPosition = PositionToCubeLocation(transform.position);
        if (oldPosition != newPosition) {
            manager.MoveBoid(oldPosition, newPosition, this);
        }
    }

    bool IsColliding(Vector3 direction) {
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
            Color lineColor = IsColliding(vec) ? Color.red : Color.green;
            Debug.DrawLine(position, position + vec * _collisionSensitivity, lineColor);
        }
    }

    private Vector3Int PositionToCubeLocation(Vector3 position) {
        position /= _boxSize;
        return Vector3Int.FloorToInt(position);
    }

//     void OnDrawGizmos() {
//         Gizmos.color = new Color(1, 0, 0, 0.2f);
//         Gizmos.DrawCube((Vector3) PositionToCubeLocation(transform.position) * _boxSize + Vector3.one * _boxSize/2, new Vector3(_boxSize, _boxSize, _boxSize));
//     }
}
