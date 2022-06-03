using UnityEngine;

public class CollectablesManager : MonoBehaviour, ICollectableObserver {

    private int collectablescnt = 5;
    private int score = 0;
    private TorusGPU collectableGraphics;

    private Collectable[] collectables;
    private PlayerMovement player;

    public ComputeShader surroundCS;

    void Start() {
        ScoreOverlay.SetScore(score);
        collectableGraphics = GetComponent<TorusGPU>();
        collectables = new Collectable[collectablescnt];
        BoidManager boidManager = (BoidManager)FindObjectOfType(typeof(BoidManager));
        for (int i = 0; i < collectablescnt; i++) {
            GameObject gameObj = new GameObject("Collectable");
            Collectable collectable = gameObj.AddComponent(typeof(Collectable)) as Collectable;
            collectable.SetUp(surroundCS);
            collectable.AddObserver((ICollectableObserver)this);
            collectables[i] = collectable;
            boidManager.FollowObject(collectable);
        }
        player = FindObjectOfType<PlayerMovement>();
    }

    public void Collected() {
        score += 10;
        ScoreOverlay.SetScore(score);
    }

    public int GetScore() {
        return score;
    }

    public void SetScore(int _score) {
        score = _score;
        ScoreOverlay.SetScore(score);
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

    private void Update() {
        collectableGraphics.RunGPUKernel(collectables, player.transform.position);
    }
}
