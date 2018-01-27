using UnityEngine;
using XInputDotNetPure;
using Sirenix.OdinInspector;

[RequireComponent( typeof( PlayerMovement ) )]
public class PlayerInput : MonoBehaviour {
    [DisableInEditorMode]
    public PlayerIndex playerIndex;
    private GamePadState state, previousState;
    private PlayerMovement movement;

    private void Awake () {
        movement = GetComponent<PlayerMovement>();
    }

    private void Start () {
        state = GamePad.GetState( playerIndex );
    }

    private void Update () {
        previousState = state;
        state = GamePad.GetState( playerIndex );
        if ( !state.IsConnected ) {
            Debug.LogFormat( "Couldn't find control {0}", playerIndex );
            return; //No hay control así que no continuamos.
        }
        //Verifcamos los botones
        if ( state.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released ) {
            movement.Shout();
        }

        if ( state.Buttons.X == ButtonState.Released && previousState.Buttons.X == ButtonState.Pressed ) {
            movement.StopShout();
        }

        if ( state.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released ) {
            movement.StartRun();
        }

        if ( state.Buttons.A == ButtonState.Released && previousState.Buttons.A == ButtonState.Pressed ) {
            movement.EndRun();
        }

        Vector3 stickDelta = new Vector3( state.ThumbSticks.Left.X, 0, state.ThumbSticks.Left.Y );
        movement.Move( stickDelta );
        //Debug.LogFormat( "Stick: X: {0}, Y: {1}", stickDelta.x, stickDelta.z );
    }


}
