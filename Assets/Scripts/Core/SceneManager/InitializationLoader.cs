using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class InitializationLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO _managersScene = default;

    private void OnEnable()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Config app run on 60 HZ
#if UNITY_ANDROID
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
#endif
        Application.targetFrameRate = 60;
        // Load the persistent managers scene
        _managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadingScene;
    }

    private void LoadingScene(AsyncOperationHandle<SceneInstance> obj)
    {   
        SceneManager.UnloadSceneAsync(0);
    }

}
