using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;


public class PartySetupScene : WindowController
{
    [Inject] private UIManager uiManager;


    public void OnClose()
    {
        uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
    }
}
