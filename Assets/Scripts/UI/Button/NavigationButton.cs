using System;
using UnityEngine;
using UnityEngine.UI;

public enum SceneID
{
    Info,
    GaneSetting,
    QuitGame
}

[Serializable]
[RequireComponent(typeof(Button))]
public class NavigationButton
{
    [SerializeField] public Button button;
    [SerializeField] public SceneID id;

    public string StringID()
    {
        switch(this.id)
        {
            case SceneID.Info:
                return ScreenIds.GameInfoScene;
            case SceneID.GaneSetting:
                return ScreenIds.GameSettingsScene;
            case SceneID.QuitGame:
                return ScreenIds.PopupConfirm;
        }
        return "";
    }
}