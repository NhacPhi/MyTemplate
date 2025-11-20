using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class StartGame : MonoBehaviour
{
    [SerializeField] private GameSceneSO locationsToLoad;

    [SerializeField] private LoadEventChannelSO loadLocation = default;

    private bool hasSaveData;
    private void OnEnable()
    {
        GameEvent.OnStartNewGame += StartNewGame;
    }

    private void OnDisable()
    {
        GameEvent.OnStartNewGame -= StartNewGame;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Check PlayerSave has data

    }

    private void StartNewGame()
    {
        hasSaveData = false;
        // SaveSystem.Player => WriteEmptySaveFile
        // SaveSystem.Player =?> SetNEwGameData();
        loadLocation.RaiseEvent(locationsToLoad, false);
    }

    private void ContinuePreviousGame()
    {
        // SvaeSystem.LoadDataPlayer
        // GetLocatoin (GameSceneSO)
        //loadLocation.RaiseEvent(locationSO, _showLoadScreen);
    }
}
