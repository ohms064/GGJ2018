using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using XInputDotNetPure;

[CreateAssetMenu( menuName = "Scriptable Objects/Multiplayer Config" )]
public class GameMode : ScriptableObject {
    [TableList]
    public PlayerData[] players;
    public static GameMode currentMode;
    public void SetAsCurrentMode () {
        currentMode = this;
    }
    public int PlayerCount {
        get {
            return players.Length;
        }
    }
}
[System.Serializable]
public class PlayerData {
    [AssetsOnly]
    public PlayerInput prefab;
    public Vector3 startPosition;
    public PlayerIndex index;
}

