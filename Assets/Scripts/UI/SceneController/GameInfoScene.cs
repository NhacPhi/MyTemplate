using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GameInfoScene : WindowController
{
    [SerializeField] private Button btnClose;
    [Inject] private UIManager uiManager;
    private void Start()
    {
        btnClose.onClick.AddListener(() => { UI_Close(); uiManager.ShowPanel(ScreenIds.PanelStartGame); }) ;
    }

    public void UI_CLOSE()
    {
        base.UI_Close();
    }
}
