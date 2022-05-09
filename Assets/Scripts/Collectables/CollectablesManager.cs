using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesManager : MonoBehaviour {

    private int collectablescnt = 1;
    private static int score = 0;

    public GameObject collectiblePrefab;
    public ComputeShader surroundCS;

    void Start() {
        for (int i = 0; i < collectablescnt; i++) {
            GameObject gameObj = new GameObject("Collectable");
            Collectable collectable = gameObj.AddComponent(typeof(Collectable)) as Collectable;
            collectable.SetUp(surroundCS, collectiblePrefab);
        }
    }

    public static void UpdateScore() {
        score++;
        Debug.Log(score);
    }
}
