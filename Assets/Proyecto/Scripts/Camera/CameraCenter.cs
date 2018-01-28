using UnityEngine;
using System.Collections;

public class CameraCenter : MonoBehaviour {
    public float weight = 1, radius = 0;
    private PlayerLookAt[] players;
    
    private float inversePlayers;

    public void InitCenter (int playerCount) {
        players = new PlayerLookAt[playerCount];
        inversePlayers = 1 / (float) playerCount;
    }

    public void AddPlayer (int i, PlayerLookAt t) {
        players[i] = t;
    }

    private void Start () {
        var group = GetComponent<Cinemachine.CinemachineTargetGroup>();
        group.m_Targets = new Cinemachine.CinemachineTargetGroup.Target[players.Length];
        for(int i = 0; i < group.m_Targets.Length; i++ ) {
            group.m_Targets[i] = new Cinemachine.CinemachineTargetGroup.Target {
                target = players[i].lookAt,
                radius = radius,
                weight = weight
            };
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos () {
        if ( !UnityEditor.EditorApplication.isPlaying ) {
            return;
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere( transform.position, 0.3f );
    }
#endif

    /// <summary>
    /// The higher the value the closest the positions are.
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <returns></returns>
    private float MinimumDistanceCondition (Vector3 one, Vector3 two) {
        return Vector3.Dot( one, two ) - 0.5f * Vector3.Dot( two, two );
    }
}
