using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {
    // movement related constants
    private float _speed = 0.1f;

    // grouping related constants
    private float _closeRadius = 3f;
    private float _viewRadius = 5f;

    private float _repulsion = 4f;
    private float _attraction = 1f;

    private float _directionWeight = 1f;
    private float _stubborness = 6f;

    // collision related constants
    private int _collisionPrecision = 10;
    private float _collisionSensitivity = 1f;
    private float _magicRatio = 1 + Mathf.Sqrt(5);

    // irrespective of frames
    void FixedUpdate() {
        Vector3 wantsToGo = AnalyzeFriends();
        // DrawColliders(transform.position, primaryDirection);
        Vector3 direction = closeVectors(wantsToGo).First(vector => !IsColliding(vector));
        direction *= _speed;
        transform.up = direction;
        transform.position += direction;
    }

    Vector3 PrimaryDirection() {
        return transform.up;
    }

    Vector3 AnalyzeFriends() {
        Vector3 repulsiveAttractiveForce = Vector3.zero;
        Vector3 directionForce = Vector3.zero;
        foreach (var boid in GameObject.FindGameObjectsWithTag("Boid")) {
            float distance = (boid.transform.position - transform.position).sqrMagnitude;
            if (distance < _closeRadius * _closeRadius) {
                repulsiveAttractiveForce += (transform.position - boid.transform.position).normalized * _repulsion;
                directionForce += boid.transform.up;
            } else if (distance < _viewRadius * _viewRadius) {
                repulsiveAttractiveForce += (boid.transform.position - transform.position).normalized * _attraction;
            }
        }
        Vector3 direction = repulsiveAttractiveForce != Vector3.zero ? repulsiveAttractiveForce.normalized : transform.up;
        direction += directionForce.normalized * _directionWeight;
        direction += transform.up * _stubborness;
        return direction.normalized;
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
}
