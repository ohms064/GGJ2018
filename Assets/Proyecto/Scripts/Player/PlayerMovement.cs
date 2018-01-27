using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NPCMovement {
    public float runningSpeed = 2f, walkingSpeed = 1f;
    [Range(0.5f, 10f)]
    public float shoutRadius = 1f;
    [DisableInPlayMode, DisableInEditorMode]
    public PlayerTeam team;

    public override void Shout() {
        Debug.Log( "Shout! Shout! Let it all out" );
        isShouting = true;
        var affected = Physics.OverlapSphere( transform.position, shoutRadius );
        for ( int i = 0; i < affected.Length; i++ ) {
            //affected[i].GetComponent<NPCPlayerInteraction>().StartConversion( team );
        }
    }

    /// <summary>
    /// Called from a FixedUpdate
    /// </summary>
    /// <param name="deltaIntensity"></param>
    private void Shouting (float deltaIntensity) {
        var affected = Physics.OverlapSphere( transform.position, shoutRadius * ( 1 + deltaIntensity ) );
        for(int i = 0; i < affected.Length; i++ ) {
            //affected[i].GetComponent<NPCPlayerInteraction>().Converting( team, deltaIntensity );
        }
    }

    public void StopShout () {
        Debug.Log( "So come on!" );
        isShouting = false;
        var affected = Physics.OverlapSphere( transform.position, shoutRadius );
        for ( int i = 0; i < affected.Length; i++ ) {
            //affected[i].GetComponent<NPCPlayerInteraction>().EndConversion();
        }
    }

    public void StartRun () {
        speed = runningSpeed;
    }

    public void EndRun () {
        speed = walkingSpeed;
    }
}

public enum PlayerTeam {
        RED, GREEN, BLUE, YELLOW
}
