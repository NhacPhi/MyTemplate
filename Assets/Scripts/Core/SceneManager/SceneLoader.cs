using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

//Scene Manager
public class SceneLoader : MonoBehaviour
{
    [Inject] private UIManager _uiManager;
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private GameSceneSO _gameplayScene = default;

    [Header("Listening to")]
    [SerializeField] private LoadEventChannelSO _loadLocation = default;
    [SerializeField] private LoadEventChannelSO _loadMenu = default;
    [SerializeField] private LoadEventChannelSO _coldStartupLocation = default;

    //[Header("Broadcasting on")]
    //[SerializeField] private BoolEventChannelSO _toggleLoadingScreen = default;
    //[SerializeField] private VoidEventChannelSO _onSceneReady = default; //picked up by the SpawnSystem
    //[SerializeField] private FadeChannelSO _fadeRequestChannel = default;

    private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
    private AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

    //Parameters coming from scene loading requests
    private GameSceneSO _sceneToLoad;
    private GameSceneSO _currentlyLoadedScene;
    public GameSceneSO CurrentLoadedScene => _currentlyLoadedScene;
    private bool _showLoadingScreen;

    private SceneInstance _gameplayManagerSceneInstance = new SceneInstance();
    private float _fadeDuration = .1f;
    private bool _isLoading = false; //To prevent a new loading request while already loading a new scene

    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvent.IsSceneReady = false;
        _loadLocation.OnLoadingRequested += LoadLocation;
        _loadMenu.OnLoadingRequested += LoadMenu;
#if UNITY_EDITOR
        _coldStartupLocation.OnLoadingRequested += LocationColdStartup;
#endif
    }

    public void RestartCurrentScene()
    {
        if (_currentlyLoadedScene != null)
        {
            LoadLocation(_currentlyLoadedScene, true, false);
        }
    }

    private void OnDisable()
    {
        _loadLocation.OnLoadingRequested -= LoadLocation;
        _loadMenu.OnLoadingRequested -= LoadMenu;
#if UNITY_EDITOR
        _coldStartupLocation.OnLoadingRequested -= LocationColdStartup;
#endif
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
            //Gameplay managers is loaded synchronously
            _gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
            _gameplayManagerLoadingOpHandle.WaitForCompletion();
            _gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

            // Phát tiếng môi trường lặp lại khi vào Map di chuyển (Cold Start)
            _audioManager.PlaySFXAsync("AudioDB_Environment", true, true).Forget();

            StartGameplay();
        }
    }
#endif

    /// <summary>
    /// This function loads the location scenes passed as array parameter
    /// </summary>
    public void LoadLocation(GameSceneSO locationToLoad, bool showLoadingScreen, bool fadeScreen)
    {
        //Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
        if (_isLoading)
            return;

        _sceneToLoad = locationToLoad;
        _showLoadingScreen = showLoadingScreen;
        _isLoading = true;

        //In case we are coming from the main menu, we need to load the Gameplay manager scene first
        if (_gameplayManagerSceneInstance.Scene == null
            || !_gameplayManagerSceneInstance.Scene.isLoaded)
        {
            _gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
            _gameplayManagerLoadingOpHandle.Completed += OnGameplayManagersLoaded;
        }
        else
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }

    private void OnGameplayManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        _gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

        StartCoroutine(UnloadPreviousScene());
    }

    /// <summary>
    /// Prepares to load the main menu scene, first removing the Gameplay scene in case the game is coming back from gameplay to menus.
    /// </summary>
    private void LoadMenu(GameSceneSO menuToLoad, bool showLoadingScreen, bool fadeScreen)
    {
        //Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
        if (_isLoading)
            return;

        _sceneToLoad = menuToLoad;
        _showLoadingScreen = showLoadingScreen;
        _isLoading = true;

        Debug.Log($"[TransitionLog] SceneLoader: LoadMenu requested for {menuToLoad.name} (showLoadingScreen = {showLoadingScreen})");

        //In case we are coming from a Location back to the main menu, we need to get rid of the persistent Gameplay manager scene
        if (_gameplayManagerSceneInstance.Scene != null
            && _gameplayManagerSceneInstance.Scene.isLoaded)
            Addressables.UnloadSceneAsync(_gameplayManagerLoadingOpHandle, true);

        StartCoroutine(UnloadPreviousScene());
    }

    /// <summary>
    /// In both Location and Menu loading, this function takes care of removing previously loaded scenes.
    /// </summary>
    private IEnumerator UnloadPreviousScene()
    {
        _isLoading = true; // Khóa loading lại
        
        // 1. Bật Loading UI NGAY LẬP TỨC trước khi Unload scene cũ
        if (_showLoadingScreen)
        {
            UIEvent.OnToggleLoadingScene?.Invoke(true);
        }

        yield return new WaitForSecondsRealtime(_fadeDuration);

        // 2. UNLOAD QUYẾT LIỆT
        if (_loadingOperationHandle.IsValid())
        {
            // Phải dùng yield return để Android có thời gian dọn RAM
            var unloadOp = Addressables.UnloadSceneAsync(_loadingOperationHandle);
            yield return unloadOp;
            Debug.Log("Scene cũ đã được Unload hoàn toàn.");
        }
#if UNITY_EDITOR
        else if (_currentlyLoadedScene != null)
        {
            // Fix cho Cold Start trong Editor
            AsyncOperation op = SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.editorAsset.name);
            if (op != null) yield return op;
        }
#endif

        // Đợi 1 frame để Unity cập nhật lại Hierarchy
        yield return null;

        // 3. LOAD SCENE MỚI
        LoadNewScene();
    }

    /// <summary>
    /// Kicks off the asynchronous loading of a scene, either menu or Location.
    /// </summary>
    private void LoadNewScene()
    {
        if (_showLoadingScreen)
        {
            StartCoroutine(TrackLoadingProgress());
        }
        else
        {
            _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
            _loadingOperationHandle.Completed += OnNewSceneLoaded;
        }
    }

    private IEnumerator TrackLoadingProgress()
    {
        Debug.Log("[TransitionLog] SceneLoader: TrackLoadingProgress - Starting Addressables scene load.");
        _loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
        
        float timer = 0f;
        float minLoadingTime = 2.0f;
        float displayProgress = 0f;

        // Xác định đây có phải là lần tải cảnh đầu tiên của trò chơi (Launch -> Menu)
        bool isInitialLoad = _currentlyLoadedScene == null;

        // Vòng lặp 1: Chạy khi chưa load xong HOẶC chưa đủ thời gian tối thiểu 2s HOẶC dữ liệu chưa preload xong (đối với lần tải đầu tiên)
        // Giới hạn tiến trình hiển thị tối đa là 90-95% để tạo cảm giác "chờ phản hồi" từ ổ đĩa
        while (!_loadingOperationHandle.IsDone || timer < minLoadingTime || (isInitialLoad && !GameEvent.IsPreloadDone))
        {
            timer += Time.unscaledDeltaTime;
            
            float timeProgress = Mathf.Clamp01(timer / minLoadingTime);
            
            if (isInitialLoad && !GameEvent.IsPreloadDone)
            {
                // Dữ liệu database/save game chưa preload xong trên Android: chạy mượt tới tối đa 90%
                displayProgress = Mathf.Min(timeProgress, 0.9f);
            }
            else if (!_loadingOperationHandle.IsDone)
            {
                // Chưa load xong thực tế: chạy mượt tới tối đa 90% theo thời gian
                displayProgress = timeProgress * 0.9f;
            }
            else
            {
                // Đã load xong thực tế nhưng chưa đủ 2s tối thiểu: chạy tiếp tới tối đa 95%
                displayProgress = Mathf.Max(displayProgress, timeProgress * 0.95f);
            }
            
            UIEvent.OnUpdateLoadingProgress?.Invoke(displayProgress);
            yield return null;
        }

        Debug.Log($"[TransitionLog] SceneLoader: TrackLoadingProgress - Loop 1 finished. AddressablesIsDone={_loadingOperationHandle.IsDone}, Timer={timer}, IsPreloadDone={GameEvent.IsPreloadDone}. Animating to 100%.");

        // Vòng lặp 2: Sau khi hoàn thành tất cả điều kiện, chạy nốt phần còn lại lên 100% thật mượt
        while (displayProgress < 1f)
        {
            displayProgress += Time.unscaledDeltaTime * 2f; // Tăng nhanh từ 95% -> 100% (~0.025s)
            if (displayProgress > 1f) displayProgress = 1f;
            
            UIEvent.OnUpdateLoadingProgress?.Invoke(displayProgress);
            yield return null;
        }
        
        // Đợi thêm 1 frame để UI hiển thị cập nhật 100% trước khi đóng
        yield return null;

        Debug.Log("[TransitionLog] SceneLoader: TrackLoadingProgress - Completed! Calling OnNewSceneLoaded.");
        OnNewSceneLoaded(_loadingOperationHandle);
    }

    private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameSceneSO previousScene = _currentlyLoadedScene;

            //Save loaded scenes (to be unloaded at next load request)
            _currentlyLoadedScene = _sceneToLoad;

            Scene s = obj.Result.Scene;
            SceneManager.SetActiveScene(s);
            LightProbes.TetrahedralizeAsync();

            _isLoading = false;

            Debug.Log($"[TransitionLog] SceneLoader: OnNewSceneLoaded - Scene {s.name} loaded and set active.");

            if (_showLoadingScreen)
                UIEvent.OnToggleLoadingScene?.Invoke(false);

            //_fadeRequestChannel.FadeIn(_fadeDuration);

            // Quản lý tiếng môi trường lặp lại (Location -> Play, Menu/Battle -> Stop)
            if (_currentlyLoadedScene != null)
            {
                if (_currentlyLoadedScene.sceneType == GameSceneType.Location)
                {
                    // Chỉ phát nếu trước đó không phải là Location (tránh ngắt quãng/đổi bài giữa các map)
                    if (previousScene == null || previousScene.sceneType != GameSceneType.Location)
                    {
                        _audioManager.PlaySFXAsync("AudioDB_Environment", true, true).Forget();
                    }
                }
                else if (_currentlyLoadedScene.sceneType == GameSceneType.Battle)
                {
                    _audioManager.StopSFX();
                }
            }

            StartGameplay();
        }    
    }

    private void StartGameplay()
    {
        Debug.Log("[TransitionLog] SceneLoader: StartGameplay - Setting IsSceneReady = true and invoking GameEvent.OnSceneReady.");
        GameEvent.IsSceneReady = true;
        GameEvent.OnSceneReady?.Invoke(); //Spawn system will spawn the PigChef in a gameplay scene
    }

    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit!");
    }
}
