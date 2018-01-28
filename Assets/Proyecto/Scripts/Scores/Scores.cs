using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scores : MonoBehaviour {
    private Text scoreText;
    [HideInInspector]
    public int score = 0;

    private void Awake () {
        scoreText = GetComponent<Text>();
        AddScore( 0 );
    }

    public void AddScore(int deltaScore) {
        score += deltaScore;
        scoreText.text = score.ToString();
    }
}
