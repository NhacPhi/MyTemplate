using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class StartGame : MonoBehaviour
{
    [SerializeField] private GameSceneSO locationsToLoad;

    [SerializeField] private LoadEventChannelSO loadLocation = default;

    [SerializeField] private List<GameSceneSO> _availableLocations = new List<GameSceneSO>();

    [Inject] private SaveSystem _saveSystem;

    private bool hasSaveData;

    private void Awake()
    {
        RegisterAllLocations();
    }

    private void OnEnable()
    {
        GameEvent.OnStartNewGame += StartNewGame;
    }

    private void OnDisable()
    {
        GameEvent.OnStartNewGame -= StartNewGame;
    }

    private void Start()
    {
        RegisterAllLocations();
    }

    private void RegisterAllLocations()
    {
        if (locationsToLoad != null)
        {
            SceneLoader.RegisterScene(locationsToLoad);
        }
        if (_availableLocations != null)
        {
            foreach (var loc in _availableLocations)
            {
                if (loc != null)
                {
                    SceneLoader.RegisterScene(loc);
                }
            }
        }
    }

    private void StartNewGame()
    {
        RegisterAllLocations();

        SaveSystem saveSys = _saveSystem ?? SaveSystem.Instance;
        GameSceneSO targetScene = null;

        if (saveSys != null && saveSys.Player != null && saveSys.Player.WorldState != null)
        {
            string savedSceneName = saveSys.Player.WorldState.LastSceneName;
            if (!string.IsNullOrEmpty(savedSceneName))
            {
                targetScene = SceneLoader.GetRegisteredScene(savedSceneName);
                if (targetScene != null)
                {
                    Debug.Log($"[StartGame] Found saved scene: '{savedSceneName}'. Loading saved scene.");
                }
                else
                {
                    Debug.LogWarning($"[StartGame] Saved scene '{savedSceneName}' found in save data, but not registered in SceneLoader. Falling back to default location.");
                }
            }
        }

        if (targetScene == null)
        {
            targetScene = locationsToLoad;
            Debug.Log($"[StartGame] Loading default location: '{(locationsToLoad != null ? locationsToLoad.name : "null")}'");
        }

        if (targetScene != null)
        {
            loadLocation.RaiseEvent(targetScene, true);
        }
        else
        {
            Debug.LogError("[StartGame] Failed to load scene: targetScene is null!");
        }
    }

    private void ContinuePreviousGame()
    {
        StartNewGame();
    }
}
