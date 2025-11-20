using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class EditorColdStartup : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private GameSceneSO thisSceneSO = default;
    [SerializeField] private GameSceneSO persistentManagersSO = default;


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
        // do something
        GameEvent.OnLoadColdStartupLocation?.Invoke(thisSceneSO, false, false);
    }
#endif
}
