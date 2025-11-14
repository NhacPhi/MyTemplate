using deVoid.Utils;
using UIFramework;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UISettings _defaultUISettings = null;

    private UIFrame _uiFrame;

    private string currentWindow;
    private string currentPanel;

    private void OnEnable()
    {
        UIEvent.OnClickNavigationButton += OnNavigatePanelStartGame;
    }

    private void OnDisable()
    {
        UIEvent.OnClickNavigationButton -= OnNavigatePanelStartGame;
    }
    private void Awake()
    {

    }

    public void Init()
    {
        _uiFrame = _defaultUISettings.CreateUIInstance();
    }

    public void OpenWindowScene(string id)
    {
        currentWindow = id;
        _uiFrame.OpenWindow(currentWindow);
    }

    public void CloseWindowScene()
    {
        _uiFrame.CloseWindow(currentWindow);
    }

    public void ShowPanel(string id)
    {
        currentPanel = id;
        _uiFrame.ShowPanel(id);
    }
    public void HidePanel()
    {
        _uiFrame.HidePanel(currentPanel);
    }

    public void CloseAllWindows()
    {
        _uiFrame.CloseAllWindows();
    }

    private void OnNavigatePanelStartGame(string id)
    {
        _uiFrame.HidePanel(ScreenIds.PanelStartGame);

        switch(id)
        {
            case ScreenIds.GameInfoScene:
                _uiFrame.OpenWindow(id);
                break;
            case ScreenIds.GameSettingsScene:
                _uiFrame.OpenWindow(id);
                break;
            case ScreenIds.PopupConfirm:
                Action cancle = () => { _uiFrame.ShowPanel(ScreenIds.PanelStartGame); };
                Action confirm = () => {
                    Application.Quit();
                };
                string message = LocalizationManager.Instance.GetLocalizedValue("UI_QUIT_QUESTION");
                ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties("Remind", message, "Confirm", "Cancel", confirm, cancle);
                _uiFrame.OpenWindow(id, popupProps);
                break;
            default:
                _uiFrame.OpenWindow(id);
                break;
        }
    }

    public void ShowPopupConfirmSettings(PopupSettingProperties popup)
    {
        _uiFrame.OpenWindow(ScreenIds.PopupConfirmSettings, popup);
    }
}
