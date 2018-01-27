using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Movement : MonoBehaviour {
    public float speed;
    protected Rigidbody rb;
    protected bool isShouting = false;

    protected virtual void Awake () {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Move (Vector3 delta) {
        if ( isShouting ) {
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
