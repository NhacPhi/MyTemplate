using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class InitializationLoader : MonoBehaviour
{
    [SerializeField] private GameSceneSO _managersScene = default;

    [SerializeField] private GameSceneSO _menuToLoad = default;


    [Header("Broadcasting on")]
    [SerializeField] private AssetReference _menuLoadChannel = default;

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
        _managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
    }

    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {
        _menuLoadChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
    }

    private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        obj.Result.RaiseEvent(_menuToLoad, true);

        SceneManager.UnloadSceneAsync(0); //Initialization is the only scene in BuildSettings, thus it has index 0
    }

}
