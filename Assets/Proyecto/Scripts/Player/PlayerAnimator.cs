using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour {
    private Animator anim;

    private void Awake () {
        anim = GetComponent<Animator>();
    }

    public void Move ( float speedNormalized ) {
        anim.SetFloat( "Speed", speedNormalized );
    }

    public void ShoutStart () {
        anim.SetBool( "IsScreaming", true );
    }

    public void ShoutEnd () {
        anim.SetBool( "IsScreaming", false );
    }
}
