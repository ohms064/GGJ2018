using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( SceneDataScriptable ) )]
class SceneDataScriptableEditor : BaseSceneEditor {
    public override void OnInspectorGUI () {
        SceneDataScriptable script = target as SceneDataScriptable;
        DrawDefaultInspector();
        script.sceneId = EditorGUILayout.Popup( "Scene To Load", script.sceneId, scenes );

        GUILayout.Space( 5 );

        if ( GUILayout.Button( "Reload Scenes" ) ) {
            string sceneId = scenes[script.sceneId];
            LoadScenes();
            script.sceneId = Find( sceneId );
        }
    }

    private Vector2 CreateSize (Texture texture, float max) {
        Vector2 size = new Vector2( texture.width, texture.height );
        while ( size.x >= max && size.y >= max ) {
            size.x *= 0.5f;
            size.y *= 0.5f;
        }

        return size;
    }
}

public class BaseSceneEditor : Editor {

    protected string[] scenes;

    private void OnEnable () {
        LoadScenes();
    }

    protected void LoadScenes () {
        scenes = ( from scene in EditorBuildSettings.scenes where scene.enabled select GetSceneName( scene ) ).ToArray();
        EditorUtility.SetDirty( target );
    }

    string GetSceneName (EditorBuildSettingsScene scene) {
        string output;
        int index = scene.path.LastIndexOf( '/' );
        output = scene.path.Substring( index + 1 );
        int index2 = output.LastIndexOf( "." );
        output = output.Substring( 0, index2 );
        return output;
    }

    protected int Find (string str) {
        for ( int i = 0; i < scenes.Length; i++ ) {
            if ( scenes[i].Equals( str ) ) {
                return i;
            }
        }
        return 0;
    }

}