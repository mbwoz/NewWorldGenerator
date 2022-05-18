using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoidObserver {

    public void BoidMoved(Boid boid, Vector3 prev, Vector3 next);

}
