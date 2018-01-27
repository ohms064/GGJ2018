using System;
using UnityEngine;

[CreateAssetMenu( menuName = "ScriptableObjects/SceneData" )]
public class SceneDataScriptable : ScriptableObject {
    public string levelName;
    [HideInInspector] public int sceneId;
    public SceneLoaderScriptable sceneManager;

    public void LoadScene () {
        sceneManager.LoadScene( this );
    }
}