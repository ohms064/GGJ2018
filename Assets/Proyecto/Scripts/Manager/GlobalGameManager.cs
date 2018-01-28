using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GlobalGameManager : MonoBehaviour {
    public CameraCenter camCenter;
#if UNITY_EDITOR
    public bool useTest;
    public GameMode test;
    
#endif
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
    [SceneObjectsOnly]
    public Scores[] scores = new Scores[4];

	// Use this for initialization
	private void Awake () {
        switch ( Current.teamsCount ) {
            case XInputDotNetPure.PlayerIndex.Two:
            case XInputDotNetPure.PlayerIndex.One:
                scores[2].gameObject.SetActive( false );
                scores[3].gameObject.SetActive( false );
                break;
            case XInputDotNetPure.PlayerIndex.Three:
                scores[3].gameObject.SetActive( false );
                break;
        }

        camCenter.InitCenter( Current.PlayerCount );
        for ( int i = 0; i < Current.PlayerCount; i++ ) {
            var currentPlayer = Current.players[i];
            var inputManager = Instantiate( currentPlayer.prefab, currentPlayer.startPosition, Quaternion.identity );
            camCenter.AddPlayer( i, inputManager.transform );
            inputManager.playerIndex = currentPlayer.index;
            var movement = inputManager.GetComponent<PlayerMovement>();
            movement.team = currentPlayer.team;
            movement.AddToScore += CreateScoreListener( scores[i] );
        }
    }

    public System.Action<int> CreateScoreListener (Scores score) {
        return score.AddScore;
    }
}
