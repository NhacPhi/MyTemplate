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

    void Start()
    {
        // Config app run on 60 HZ
#if UNITY_ANDROID
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
#endif
        Application.targetFrameRate = 60;

        _managersScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
    }

    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {
        _menuLoadChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
    }

    private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        Debug.Log("[TransitionLog] InitializationLoader: LoadMainMenu - Raising menu load event (showLoadingScreen = true)");
        // Kích hoạt việc hiển thị LaunchLoadingScene với tiến trình load
        obj.Result.RaiseEvent(_menuToLoad, true);

        // Giải thích lý do trì hoãn Unload Scene 0 và đóng loading thủ công:
        // - Do LaunchLoadingScene được cấu hình HideOnForegroundLost = false, nó sẽ không tự động đóng
        //   khi StartGameScene được mở. Điều này giữ màn hình loading luôn hiển thị che phủ game.
        // - Ta đăng ký OnSceneReady, chờ thêm 0.2 giây để đảm bảo StartGameScene đã được dựng xong dưới nền.
        // - Sau 0.2s, ta mới gọi đóng LaunchLoadingScene và giải phóng Scene 0.
        //   Kết quả là màn hình loading biến mất mượt mà và hiển thị ngay Menu hoàn thiện, không có frame trống.
        Action onReady = null;
        onReady = () =>
        {
            GameEvent.OnSceneReady -= onReady;
            Debug.Log("[TransitionLog] InitializationLoader: OnSceneReady caught. Starting DelayUnloadScene0.");
            StartCoroutine(DelayUnloadScene0());
        };
        GameEvent.OnSceneReady += onReady;
    }

    private IEnumerator DelayUnloadScene0()
    {
        Debug.Log("[TransitionLog] InitializationLoader: DelayUnloadScene0 - Waiting 0.2s before transition.");
        yield return new WaitForSecondsRealtime(0.2f);
        
        Debug.Log("[TransitionLog] InitializationLoader: DelayUnloadScene0 - Synchronously closing loading UI and opening menu UI.");
        
        // Đóng loading và mở StartGameScene trong cùng 1 frame để tránh lệch nhịp render của GPU
        if (UIManager.Instance != null)
        {
            Debug.Log("[TransitionLog] InitializationLoader: UIManager.Instance is valid. Triggering CloseWindowScene, OpenWindowScene and ShowPanel.");
            UIManager.Instance.CloseWindowScene(ScreenIds.LaunchLoadingScene);
            UIManager.Instance.OpenWindowScene(ScreenIds.StartGameScene);
            UIManager.Instance.ShowPanel(ScreenIds.PanelStartGame);
        }
        else
        {
            Debug.LogError("[TransitionLog] InitializationLoader: UIManager.Instance is null!");
        }
        
        SceneManager.UnloadSceneAsync(0);
    }
}
