using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class EditorColdStartup : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private GameSceneSO thisSceneSO = default;
    [SerializeField] private GameSceneSO persistentManagersSO = default;
    [SerializeField] private AssetReference notifyColdStartupChannel = default;


    private bool isColdStart = false;

    private void Awake()
    {
        if (!SceneManager.GetSceneByName(persistentManagersSO.sceneReference.editorAsset.name).isLoaded)
        {
            isColdStart = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isColdStart)
        {
            persistentManagersSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;

        }
    }

    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {
        notifyColdStartupChannel.LoadAssetAsync<LoadEventChannelSO>().Completed += OnNotifyChannelLoaded;
    }

    private void OnNotifyChannelLoaded(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        if (thisSceneSO != null)
        {
            obj.Result.RaiseEvent(thisSceneSO);
        }
        else
        {
            //Raise a fake scene ready event, so the player is spawned
            //onSceneReadyChannel.RaiseEvent();
            //When this happens, the player won't be able to move between scenes because the SceneLoader has no conception of which scene we are in
        }
    }
#endif
}
