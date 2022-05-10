using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesManager : MonoBehaviour, ICollectableObserver {

    private int collectablescnt = 1;
    private int score = 0;

    private Collectable[] collectables;

    public GameObject collectiblePrefab;
    public ComputeShader surroundCS;

    void Start() {
        collectables = new Collectable[collectablescnt];

        for (int i = 0; i < collectablescnt; i++) {
            GameObject gameObj = new GameObject("Collectable");
            Collectable collectable = gameObj.AddComponent(typeof(Collectable)) as Collectable;
            collectable.SetUp(surroundCS, collectiblePrefab);
            collectable.AddObserver((ICollectableObserver)this);
            collectables[i] = collectable;
        }
    }

    public void Collected() {
        score++;
        Debug.Log(score);
    }

    public int GetScore() {
        return score;
    }

    public void SetScore(int _score) {
        score = _score;
    }

    public Vector3[] GetCollectables() {
        Vector3[] positions = new Vector3[collectablescnt];

        for (int i = 0; i < collectablescnt; i++) {
            positions[i] = collectables[i].transform.position;
        }

        return positions;
    }

    public void SetCollectables(Vector3[] positions) {
        for (int i = 0; i < collectablescnt; i++) {
            collectables[i].transform.position = positions[i];
        }
    }
}
