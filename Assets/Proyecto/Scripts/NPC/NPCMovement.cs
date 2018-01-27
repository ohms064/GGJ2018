using UnityEngine;
using System.Collections;

public class NPCMovement : MonoBehaviour {
    public float speed;
    protected Rigidbody rb;
    protected bool isShouting = false;

    protected virtual void Awake () {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Move (Vector3 delta) {
        if ( isShouting ) {
            Debug.Log( "Shouting, can't move" );
            return;
        }
        Vector3 targetPosition = delta * Time.deltaTime * speed;
        targetPosition += rb.position;
        rb.MovePosition( targetPosition );
    }

    public virtual void Shout () {
        isShouting = true;
    }
}
