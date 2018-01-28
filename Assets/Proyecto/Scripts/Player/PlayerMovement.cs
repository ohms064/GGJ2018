using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof( PlayerAnimator ) )]
public class PlayerMovement : Movement {
    public float runningSpeed = 2f, walkingSpeed = 1f;
    [Range(0.5f, 10f), DisableInPlayMode]
    public float shoutRadius = 1f;
    private float squareShoutRadius;
    [DisableInPlayMode, DisableInEditorMode]
    public PlayerTeam team = PlayerTeam.NONE;
    public int layerMask;
    public event System.Action<int> AddToScore;
    [Range(50, 500, order = 10)]
    public int scorePerShout;
    private PlayerAnimator animator;
    public float accelerationTime = 1f;
    private float inverseMaxSpeed;

    protected override void Awake () {
        base.Awake();
        animator = GetComponent<PlayerAnimator>();
        inverseMaxSpeed = 1 / runningSpeed;
    }

    public override void Move (Vector3 delta) {
        base.Move( delta );
        animator.Move( speed * inverseMaxSpeed * delta.normalized.magnitude );
        var scale = transform.localScale;
        if (delta.x > 0 && scale.x < 0) {
            scale.x *= -1f;
            transform.localScale = scale;
        }
        else if ( delta.x < 0 && scale.x > 0 ) {
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void Start () {
        speed = walkingSpeed;
        layerMask = LayerMask.GetMask( "NPC" );
        squareShoutRadius = shoutRadius * shoutRadius;
    }

    public override void Shout() {
        Debug.Log( "Shout! Shout! Let it all out" );
        isShouting = true;
        animator.ShoutStart();
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
            if(AddToScore != null ) {
                AddToScore( (int) (scorePerShout * deltaIntensity) );
            }
        }
    }

    public void StopShout () {
        Debug.Log( "So come on!" );
        isShouting = false;
        var affected = Physics.OverlapSphere( transform.position, shoutRadius, layerMask );
        for ( int i = 0; i < affected.Length; i++ ) {
            affected[i].GetComponent<NPCPlayerInteraction>().EndConversion();
        }
        animator.ShoutEnd();
    }

    public void StartRun () {
        StartCoroutine( LerpSpeed( walkingSpeed, runningSpeed ) );
    }

    public void EndRun () {
        StartCoroutine( LerpSpeed( runningSpeed, walkingSpeed ) );
    }

    private IEnumerator LerpSpeed(float a, float b) {
        float t = 0f;
        float inverseAccelerationTime = 1 / accelerationTime;
        while ( t < accelerationTime ) {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            speed = Mathf.Lerp( a, b, t * inverseAccelerationTime );
        }
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
