using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AutoSaveManager : MonoBehaviour
{
    [Inject] private SaveSystem _saveSystem;
    private void OnApplicationQuit()
    {
        //_saveSystem.SaveDataToDisk(GameSaveType.All);
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            //_saveSystem.SaveDataToDisk(GameSaveType.All);
        }
    }
}
