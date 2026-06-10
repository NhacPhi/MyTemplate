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
        btnPlayGame.onClick.AddListener(async () => { 
            uiManager.HidePanel(ScreenIds.PanelStartGame);
            GameEvent.OnStartNewGame?.Invoke();

            var tcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            System.Action onSceneReady = () => tcs.TrySetResult();
            GameEvent.OnSceneReady += onSceneReady;
            await tcs.Task;
            GameEvent.OnSceneReady -= onSceneReady;

            uiManager.OpenWindowScene(ScreenIds.GamePlayScene); 
        });

    }
}
