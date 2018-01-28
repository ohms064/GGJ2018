using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : Movement {

    public LayerMask playerLayer;
    private NavMeshAgent navMeshAgent;
    private Vector3 direction;
    public  NPCAnimator anim;
    //private 

#if UNITY_EDITOR
    private void Reset () {
        anim = GetComponent<NPCAnimator>();
    }
#endif

    protected override void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start() {
        transform.Rotate(Vector3.up, Random.Range(0,360), Space.Self);
        direction = GetComponent<Transform>().forward;
        InvokeRepeating("CheckSurroundings", 0.1f, 1.0f);
    }

    void FixedUpdate() {
        if (isShouting) {
            //Dont move
            CancelInvoke();
        } else {
            navMeshAgent.Move(direction * Time.deltaTime * speed);
            var scale = transform.localScale;
            if ( direction.x > 0 && scale.x < 0 ) {
                scale.x *= -1f;
                transform.localScale = scale;
            }
            else if ( direction.x < 0 && scale.x > 0 ) {
                scale.x *= -1f;
                transform.localScale = scale;
            }
        }
        anim.SetWalk( navMeshAgent.speed );
    }

    void CheckSurroundings() {
        Collider[] players = Physics.OverlapSphere(GetComponent<Transform>().position, 5.0f, playerLayer);
        for(int i=0; i<players.Length; i++) {
            direction = players[i].GetComponent<Transform>().position - GetComponent<Transform>().position;
            direction = direction.normalized*-1;
            //navMeshAgent.SetDestination(players[i].GetComponent<Transform>().position*-1);
        }
        
    }
}
