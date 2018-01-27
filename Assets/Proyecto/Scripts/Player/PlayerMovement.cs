using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NPCMovement {
    public float runningSpeed = 2f, walkingSpeed = 1f;
    [Range(0.5f, 10f), DisableInPlayMode]
    public float shoutRadius = 1f;
    private float squareShoutRadius;
    [DisableInPlayMode, DisableInEditorMode]
    public PlayerTeam team = PlayerTeam.NONE;
    public int layerMask;

    private void Start () {
        speed = walkingSpeed;
        layerMask = LayerMask.GetMask( "NPC" );
        squareShoutRadius = shoutRadius * shoutRadius;
    }

    public override void Shout() {
        Debug.Log( "Shout! Shout! Let it all out" );
        isShouting = true;
        var affected = Physics.OverlapSphere( transform.position, shoutRadius, layerMask );
        for ( int i = 0; i < affected.Length; i++ ) {
            affected[i].GetComponent<NPCPlayerInteraction>().StartConversion( team );
        }
    }

    /// <summary>
    /// Called from a FixedUpdate
    /// </summary>
    /// <param name="deltaIntensity"></param>
    public void Shouting (float deltaIntensity) {
        var affected = Physics.OverlapSphere( transform.position, shoutRadius * ( 1 + deltaIntensity ), layerMask );
        for(int i = 0; i < affected.Length; i++ ) {
            var distance = ( transform.position - affected[i].transform.position ).sqrMagnitude;
            var lerpDistance = Mathf.InverseLerp( squareShoutRadius, 0f, distance );
            affected[i].GetComponent<NPCPlayerInteraction>().Converting( team, lerpDistance );
        }
    }

    public void StopShout () {
        Debug.Log( "So come on!" );
        isShouting = false;
        var affected = Physics.OverlapSphere( transform.position, shoutRadius, layerMask );
        for ( int i = 0; i < affected.Length; i++ ) {
            affected[i].GetComponent<NPCPlayerInteraction>().EndConversion();
        }
    }

    public void StartRun () {
        speed = runningSpeed;
    }

    public void EndRun () {
        speed = walkingSpeed;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos () {
        Color color = Color.green;
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.DrawSphere( transform.position, shoutRadius );
    }
#endif
}

public enum PlayerTeam {
        RED, GREEN, BLUE, YELLOW, NONE
}
