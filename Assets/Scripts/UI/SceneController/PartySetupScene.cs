using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;


public class PartySetupScene : WindowController
{
    [Inject] private UIManager uiManager;


    public void OnClose()
    {
        if (uiManager != null)
        {
            uiManager.CloseWindowScene(ScreenIds.PartySetupScene);
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        }
        else
        {
            UI_Close();
        }
    }
}
