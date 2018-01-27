using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour {
#if UNITY_EDITOR
    public bool useTest;
    public GameMode test;
#endif
    private float[] scores;
    private GameMode Current {
        get {
#if UNITY_EDITOR
            if ( useTest ) {
                return test;
            }
#endif
            return GameMode.currentMode;
        }
    }

	// Use this for initialization
	private void Awake () {
        scores = new float[Current.PlayerCount];
        for ( int i = 0; i < Current.PlayerCount; i++ ) {
            var currentPlayer = Current.players[i];
            var inputManager = Instantiate( currentPlayer.prefab, currentPlayer.startPosition, Quaternion.identity );
            inputManager.playerIndex = currentPlayer.index;
            inputManager.GetComponent<PlayerMovement>().team = currentPlayer.team;
        }

	}
}
