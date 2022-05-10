using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData {

    private int score;
    private float[,] collectables;
    private float[] player;

    public int GetScore() {
        return score;
    }

    public void SetScore(int _score) {
        score = _score;        
    }

    public Vector3[] GetCollectables() {
        Vector3[] positions = new Vector3[collectables.GetLength(0)];
        for (int i = 0; i < positions.Length; i++) {
            positions[i].x = collectables[i, 0];
            positions[i].y = collectables[i, 1];
            positions[i].z = collectables[i, 2];
        }
        return positions;
    }

    public void SetCollectables(Vector3[] _collectables) {
        collectables = new float[_collectables.Length, 3];
        for (int i = 0; i < collectables.GetLength(0); i++) {
            collectables[i, 0] = _collectables[i].x;
            collectables[i, 1] = _collectables[i].y;
            collectables[i, 2] = _collectables[i].z;
        }
    }

    public Vector3 GetPlayer() {
        return new Vector3(player[0], player[1], player[2]);
    }

    public void SetPlayer(Vector3 _position) {
        player = new float[3] {_position.x, _position.y, _position.z};
    }
}
