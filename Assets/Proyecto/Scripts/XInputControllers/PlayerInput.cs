using UnityEngine;
using XInputDotNetPure;
using Sirenix.OdinInspector;

[RequireComponent( typeof( PlayerMovement ) )]
public class PlayerInput : MonoBehaviour {
    [DisableInEditorMode]
    public PlayerIndex playerIndex;
    public float vibrationMultiplier = 1f;
    private GamePadState state, previousState;
    private PlayerMovement movement;
    private bool tapping;
    private float tapTime;
    [Range(.01f, 1f)]
    public float maxButtonTime = 0.5f;
    private float vibrationIntensity;

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
        if ( tapping ) {
            tapTime += Time.deltaTime;
            if(tapTime > maxButtonTime ) {
                tapping = false;
                movement.StopShout();
            }
            else if( state.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released ) {
                vibrationIntensity = Mathf.InverseLerp( 0f, maxButtonTime, tapTime );
                tapTime = 0f;
            }
        }
        else {
            if ( state.Buttons.X == ButtonState.Pressed && previousState.Buttons.X == ButtonState.Released ) {
                movement.Shout();
                tapping = true;
                tapTime = 0f;
            }
        }
        
        
        if ( state.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released ) {
            movement.StartRun();
        }

        if ( state.Buttons.A == ButtonState.Released && previousState.Buttons.A == ButtonState.Pressed ) {
            movement.EndRun();
        }

        Vector3 stickDelta = new Vector3( state.ThumbSticks.Left.X, 0, state.ThumbSticks.Left.Y );
        movement.Move( stickDelta );
        //Debug.LogFormat( "{2} Stick: X: {0}, Y: {1}", stickDelta.x, stickDelta.z, playerIndex );
    }

    private void FixedUpdate () {
        if ( tapping ) {
            var vibration = vibrationIntensity * vibrationMultiplier;
            Debug.LogFormat( "Vibration intensity: {0}", vibration );
            GamePad.SetVibration( playerIndex, vibration, vibration );
        }
        else {
            GamePad.SetVibration( playerIndex, 0, 0 );
        }
    }

}
