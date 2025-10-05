using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;
using Observer;

//Scene Manager
public class SceneLoader : MonoBehaviour
{
    [Inject] private UIManager _uiManager;
    [SerializeField] private GameSceneSO _sceneLocation = default;

    //Handle event load scene

    //Parameters coming from scene loaidng request
    private GameSceneSO _sceneToLoad;
    private GameSceneSO _currentlyLoadedScene;

    private SceneInstance _gameplayManagerSceneInstance = new SceneInstance();
    private bool _isLoading = false; //To prevent a new loading request while already loading a new scene
    private bool _isShowLoading = false;

    private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
    private AsyncOperationHandle<SceneInstance> _gamePlayManagerOprerationHandle;

    private void OnEnable()
    {
        GameEvent.OnLoadSceneLocation += LoadLocation;
#if UNITY_EDITOR
        GameEvent.OnLoadColdStartupLocation += LocationColdStartup;
#endif
    }

    private void OnDisable()
    {
        GameEvent.OnLoadSceneLocation -= LoadLocation;
#if UNITY_EDITOR
        GameEvent.OnLoadColdStartupLocation -= LocationColdStartup;
#endif
    }

    private void LoadLocation(GameSceneSO locationToLoad, bool showloadingScene = false, bool fadeScene = false)
    {
        if (_isLoading)
            return;

        _isLoading = true;
        _isShowLoading = showloadingScene;
        _sceneToLoad = locationToLoad;

        //Show loading screen
        if(_isShowLoading)
        {
            _uiManager.OpenCurrentScene(ScreenIds.LoadingScene);
        }


        StartCoroutine(UnLoadPreviousScene());
    }

#if UNITY_EDITOR
    /// <summary>
    /// This special loading function is only used in the editor, when the developer presses Play in a Location scene, without passing by Initialisation.
    /// </summary>
    private void LocationColdStartup(GameSceneSO currentlyOpenedLocation, bool showLoadingScreen, bool fadeScreen)
    {
        _currentlyLoadedScene = currentlyOpenedLocation;

        if (_currentlyLoadedScene.sceneType == GameSceneType.Location)
        {

        }
    }
#endif

    private IEnumerator UnLoadPreviousScene()
    {
        yield return new WaitForSeconds(0.5f);

        if(_currentlyLoadedScene != null) // would be null if the game was started in Initialization
        {
            if(_currentlyLoadedScene.sceneReference.OperationHandle.IsValid())
            {
                _currentlyLoadedScene.sceneReference.UnLoadScene();
            }
#if UNITY_EDITOR
            else
            {
                SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.editorAsset.name);
            }
#endif
        }

        LoadNewScene();
    }

    private void LoadNewScene()
    {
        //if (_showLoadingScreen)
        //{
        //    _toggleLoadingScene.RaiseEvent(true);
        //}

        _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);

        if (_isShowLoading)
        {
            _uiManager.CloseCurrentScene();
        }

        _loadingOperationHandle.Completed += OnNewSceneLoaded;
    }

    private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        _currentlyLoadedScene = _sceneToLoad;

        Scene s = obj.Result.Scene;
        SceneManager.SetActiveScene(s);
        LightProbes.TetrahedralizeAsync();

        _isLoading = false;
        // Call loading scene
        //StartCoroutine(StartGameplay());
    }
}
