using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : Movement {

    private NavMeshAgent navMeshAgent;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void FixedUpdate() {
        navMeshAgent.Move(Vector3.forward);
    }
}
