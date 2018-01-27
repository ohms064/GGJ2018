using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {
    private Rigidbody rb;
    private float speed = 1f;
    public float runningSpeed = 2f, walkingSpeed = 1f;
    [Range(0.5f, 10f)]
    public float shoutRadius = 1f;
    private bool isShouting = false;

    private void Awake () {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 delta) {
        if ( isShouting ) {
            return;
        }
        Vector3 targetPosition = delta * Time.deltaTime * speed;
        targetPosition += rb.position;
        rb.MovePosition( targetPosition );
    }

    public void Shout() {
        Debug.Log( "Shout! Shout! Let it all out" );
        isShouting = true;
    }

    private void Shouting () {

    }

    public void StopShout () {
        Debug.Log( "So come on!" );
        isShouting = false;
    }

    public void StartRun () {
        speed = runningSpeed;
    }

    public void EndRun () {
        speed = walkingSpeed;
    }
}
