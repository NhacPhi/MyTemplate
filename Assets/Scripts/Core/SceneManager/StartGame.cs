using UnityEngine;

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
        loadLocation.RaiseEvent(locationsToLoad, true);
    }

    private void ContinuePreviousGame()
    {
        // SvaeSystem.LoadDataPlayer
        // GetLocatoin (GameSceneSO)
        //loadLocation.RaiseEvent(locationSO, _showLoadScreen);
    }
}
