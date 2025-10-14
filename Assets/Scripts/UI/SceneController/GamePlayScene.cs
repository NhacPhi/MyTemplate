using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GamePlayScene : WindowController
{
    [SerializeField] private Button btnPlayerInfo;

    [Inject] private UIManager uiManager;
    private void Start()
    {
        btnPlayerInfo.onClick.AddListener(() =>
        {
            uiManager.CloseAllWindows();
            uiManager.ShowPanel(ScreenIds.GamePlayPanel);
        });
    }
}
