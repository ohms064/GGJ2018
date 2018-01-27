using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class NPCPlayerInteraction : MonoBehaviour {
    [Range(1f, 50f)]
    public float sanity, conversion;
    private float currentSanity, currentConversion;
    private PlayerTeam currentPlayer = PlayerTeam.NONE, convertingPlayer = PlayerTeam.NONE;
    private bool converting;
    private NPCMovement movement;
    [Range(0.1f, 2f)]
    public float conversionRate;

    private void Awake () {
        movement = GetComponent<NPCMovement>();    
    }

    public void StartConversion (PlayerTeam team) {
        if ( !converting && currentPlayer != team ) {
            converting = true;
            convertingPlayer = team;
        }
    }

    public void Converting(PlayerTeam team, float delta) {
        if ( convertingPlayer == PlayerTeam.NONE ) {
            StartConversion( team );
        }
        if ( !converting || team != convertingPlayer ) {
            return;
        }
        currentConversion += delta * conversionRate;
        Debug.LogFormat( "Converting: {2} {0} {1}", delta, convertingPlayer, currentConversion );
        if(currentConversion >= conversion ) {
            Convert( team );
        }
    }

    public void Convert(PlayerTeam team) {
        Debug.LogFormat( "Converted {0}", team );
        currentPlayer = team;
        currentConversion = 0f;
        EndConversion();
    }

    public void EndConversion () {
        converting = false;
        convertingPlayer = PlayerTeam.NONE;
    }
}
