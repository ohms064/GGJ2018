using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[RequireComponent(typeof(NPCMovement))]
public class NPCAgent : MonoBehaviour {
    [MinMaxSlider(0, 20, true)]
    public Vector2 timeRange;
    public float Time {
        get {
            return Random.Range( timeRange.x, timeRange.y );
        }
    }

    //private Movement movement;

    private void Awake() {
        //movement = GetComponent<Movement>();
    }

    private void FixedUpdate() {
        //movement.Move(Vector3.forward);
    }


}
