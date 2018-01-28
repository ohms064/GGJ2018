using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu( menuName = "ScriptableObjects/SceneData" )]
public class SceneDataScriptable : ScriptableObject {
    public string levelName;
    [HideInInspector] public int sceneId;

    public void LoadScene () {
        SceneManager.LoadScene(sceneId);
    }
}