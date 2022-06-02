using UnityEngine;
using UnityEngine.UI;

public class ScoreOverlay : MonoBehaviour {
    public static void SetScore(int score) {
        Text scoreText = GameObject.Find("Score").GetComponent<Text>();
        scoreText.text = "SCORE - " + score.ToString();
    }
}
