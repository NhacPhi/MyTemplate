using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager Instance { get; private set; }

    [Inject] private SaveSystem _saveSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            SaveCurrentSceneAndGame();
        }
    }
#endif

    private void OnApplicationQuit()
    {
        SaveCurrentSceneAndGame();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveCurrentSceneAndGame();
        }
    }

    public void SaveCurrentSceneAndGame()
    {
        SaveSystem saveSys = _saveSystem ?? SaveSystem.Instance;
        if (saveSys == null || saveSys.Player == null) return;

        string sceneToSave = null;

        if (SceneLoader.LastLoadedLocation != null)
        {
            sceneToSave = SceneLoader.LastLoadedLocation.name;
        }
        else if (SceneLoader.Instance != null && SceneLoader.Instance.CurrentLoadedScene != null)
        {
            if (SceneLoader.Instance.CurrentLoadedScene.sceneType == GameSceneType.Location)
            {
                sceneToSave = SceneLoader.Instance.CurrentLoadedScene.name;
            }
        }
        else
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            var regScene = SceneLoader.GetRegisteredScene(activeSceneName);
            if (regScene != null && regScene.sceneType == GameSceneType.Location)
            {
                sceneToSave = regScene.name;
            }
            else if (!string.IsNullOrEmpty(activeSceneName) && activeSceneName != "0_Initialization" && activeSceneName != "PersistentManagement" && activeSceneName != "Menu")
            {
                sceneToSave = activeSceneName;
            }
        }

        if (!string.IsNullOrEmpty(sceneToSave))
        {
            if (saveSys.Player.WorldState != null)
            {
                saveSys.Player.WorldState.LastSceneName = sceneToSave;
                Debug.Log($"[AutoSaveManager] Saved current scene name: '{sceneToSave}' to save data.");
            }
        }

        saveSys.SaveDataToDisk(GameSaveType.All);
    }
}
