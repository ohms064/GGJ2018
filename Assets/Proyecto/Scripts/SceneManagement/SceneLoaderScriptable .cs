using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void SceneDelegate ();
public delegate void SceneSetter ();

[CreateAssetMenu( menuName = "ScriptableObjects/SceneManager" )]
public class SceneLoaderScriptable : ScriptableObject {
    public int sceneId, sceneLoader, sceneSelector, sceneMenu;
    public LoadSceneMode loadingMode = LoadSceneMode.Single;
    AsyncOperation loader;
    public static event SceneDelegate SceneLoadingFinish;
    public static event SceneDelegate StartSceneLoading;
    public SceneDataScriptable data;

    public IEnumerator LoadAsync () {

        loader = SceneManager.LoadSceneAsync( data.sceneId, loadingMode );
        loader.allowSceneActivation = false;
        while ( !loader.isDone ) {
            yield return new WaitForEndOfFrame();
            if ( loader.progress >= 0.9f ) {
                loader.allowSceneActivation = true;
            }
        }
        if ( SceneLoadingFinish != null ) {
            SceneLoadingFinish();
        }
    }

    public void StartLoading () {
        if ( StartSceneLoading != null ) {
            StartSceneLoading();
        }
    }

    public void LoadScene (SceneDataScriptable scene) {
        SceneManager.LoadScene( scene.sceneId );
        data = scene;
    }

    public void StartLoadingScreen () {
        SceneManager.LoadScene( sceneLoader );
    }

    public void StartSelectorScreen () {
        SceneManager.LoadScene( sceneSelector );
    }

    public void StartMainMenu () {
        SceneManager.LoadScene( sceneMenu );
    }

    public void QuitApplication () {
        Application.Quit();
    }
}