using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class StartGameScene : WindowController
{
    [SerializeField] private Button btnPlayGame;

    [Inject] private UIManager uiManager;

    private void Start()
    {
        btnPlayGame.onClick.AddListener(() => { 
            uiManager.OpenWindowScene(ScreenIds.GamePlayScene); 
            uiManager.HidePanel();
            GameEvent.OnStartNewGame?.Invoke();}
        );

    }
}
