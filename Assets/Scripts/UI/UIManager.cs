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
        //Signals.Get<StartSceneSignal>().AddListener(OnStartDemo);
        _uiFrame = _defaultUISettings.CreateUIInstance();
    }

    private async UniTask Init(CancellationToken token)
    {
        //var uiSettings = await AddressablesManager.Instance.LoadAssetAsync<UISettings>(
        //       AddressConstant.UISetting, token: token);

        //_defaultUISettings = uiSettings;

        //AddressablesManager.Instance.RemoveAsset(AddressConstant.UISetting);
    }

    public void OpenCurrentScene(string id)
    {
        currentWindow = id;
        _uiFrame.OpenWindow(currentWindow);
    }

    public void CloseCurrentScene()
    {
        _uiFrame.CloseWindow(currentWindow);
    }

    public void ShowPanel(string id)
    {
        _uiFrame.ShowPanel(id);
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
}
