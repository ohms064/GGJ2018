using UnityEngine;
using System.Collections;

public class NPCAnimator : MonoBehaviour {
    private Animator anim;

    private void Awake () {
        anim = GetComponent<Animator>();
    }

    public void Explode () {
        anim.SetTrigger( "Explode" );
    }

    public void SetWalk (float speed) {
        anim.SetFloat( "Speed", speed );
    }
}
