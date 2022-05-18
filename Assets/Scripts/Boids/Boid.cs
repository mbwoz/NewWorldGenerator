using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExtensionMethods;

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

    // target related constant
    private float _radius = 2f;

    private BoidManager manager;
    public Vector3 friendsDirection { private get; set; }
    public Component target { private get; set; }
    // in text-book observer design pattern this should be a list rather than a single reference
    // but performance matters here, so we prefer a reference rather than one element list
    public IBoidObserver observer { private get; set; }

    // irrespective of frames
    void FixedUpdate() {
        Vector3 direction;
        // target null checks are not necessary, they are here to make playground functional
        if (target != null && (transform.position - target.transform.position).sqrMagnitude < _radius * _radius) {
            Vector3 destination = FollowFromScratch();
            direction = destination - transform.position;
        } else {
            Vector3 destinationDirection = target != null ? (target.transform.position - transform.position).normalized : Vector3.zero;
            // friendsDirection calculated on GPU and already normalized
            Vector3 wantsToGo = (friendsDirection + transform.up * _stubborness + destinationDirection * _conscientiousness).normalized;

            direction = wantsToGo.CloseVectors(_collisionPrecision).First(vector => !IsColliding(vector)).normalized;
            transform.up = direction;
            direction *= _speed;
        }

        observer.BoidMoved(this, transform.position, transform.position + direction);
        transform.position += direction;
    }

    private Vector3 FollowFromScratch() {
        // return position to start following the target again
        return ((PlayerMovement) FindObjectOfType(typeof(PlayerMovement))).transform.position + Random.insideUnitSphere * Random.Range(10, 20);
    }

    private bool IsColliding(Vector3 direction) {
        return Physics.Raycast(transform.position, direction, _collisionSensitivity);
    }
}
