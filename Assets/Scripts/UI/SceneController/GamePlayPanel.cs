using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class GamePlayPanel : PanelController
{
    [SerializeField] private Button btnClosePanel;

    [Inject] private UIManager uiManager;

    private void Start()
    {
        btnClosePanel.onClick.AddListener(() =>
        {
            uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
            uiManager.HidePanel();
        });
    }
}
