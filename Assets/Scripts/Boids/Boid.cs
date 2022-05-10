using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {
    // movement related constants
    private float _speed = 0.1f;

    // grouping related constants
    private float _closeRadius = 3f;
    private float _viewRadius = 20f;

    private float _repulsion = 15f;
    private float _attraction = 1f;

    private float _directionWeight = 1f;
    private float _stubborness = 10f;

    // collision related constants
    private int _collisionPrecision = 20;
    private float _collisionSensitivity = 1f;
    private float _magicRatio = 1 + Mathf.Sqrt(5);

    // performance related constants
    private float boxSize = 100f;
    private BoidOptimizer optimizer;

    void Start() {
        optimizer = (BoidOptimizer) FindObjectOfType(typeof(BoidOptimizer));
        optimizer.AddBoid(PositionToCubeLocation(transform.position), this);
    }

    // irrespective of frames
    void FixedUpdate() {
        Vector3 oldPosition = PositionToCubeLocation(transform.position);
        Vector3 wantsToGo = AnalyzeFriends(optimizer.GetBoidsInCube(oldPosition));

        Vector3 direction = closeVectors(wantsToGo).First(vector => !IsColliding(vector)).normalized;
        transform.up = direction;
        direction *= _speed;
        transform.position += direction;

        Vector3 newPosition = PositionToCubeLocation(transform.position);
        if (oldPosition != newPosition) {
            optimizer.MoveBoid(oldPosition, newPosition, this);
        }
    }

    Vector3 AnalyzeFriends(HashSet<Boid> potentialFriends) {
        Vector3 repulsiveAttractiveForce = Vector3.zero;
        Vector3 directionForce = Vector3.zero;
        foreach (var boid in potentialFriends) {
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

    Vector3 PositionToCubeLocation(Vector3 position) {
        position /= boxSize;
        position = Vector3Int.FloorToInt(position);
        position *= boxSize;
        return position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(PositionToCubeLocation(transform.position) + Vector3.one * boxSize/2, new Vector3(boxSize, boxSize, boxSize));
    }
}
