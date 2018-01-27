using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
public class NPCPlayerInteraction : MonoBehaviour {
    [Range(1f, 50f)]
    public float sanity, conversion;
    private float currentSanity, currentConversion;
    private PlayerTeam currentPlayer;
    private bool converting;
    private NPCMovement movement;

    private void Awake () {
        movement = GetComponent<NPCMovement>();    
    }

    public void StartConversion (PlayerTeam team) {
        if ( !converting ) {
            converting = true;
            currentPlayer = team;
        }
    }

    public void Converting(PlayerTeam team, float delta) {
        if ( !converting || team != currentPlayer ) {
            return;
        }
        currentConversion += delta;
        if(currentConversion >= conversion ) {
            Convert( team );
        }
    }

    public void Convert(PlayerTeam team) {
        currentPlayer = team;
        currentConversion = 0f;
        EndConversion();
    }

    public void EndConversion () {
        converting = false;
    }
}
