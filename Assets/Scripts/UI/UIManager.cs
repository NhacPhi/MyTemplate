using deVoid.Utils;
using UIFramework;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using VContainer;
using VContainer.Unity;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UISettings _defaultUISettings = null;

    private UIFrame _uiFrame;
    [Inject] private IObjectResolver _objectResolver;

    private void OnEnable()
    {
        UIEvent.OnClickNavigationButton += OnNavigatePanelStartGame;
        UIEvent.OnToggleLoadingScene += ToggleLoadingScene;

    }

    private void OnDisable()
    {
        UIEvent.OnClickNavigationButton -= OnNavigatePanelStartGame;
        UIEvent.OnToggleLoadingScene -= ToggleLoadingScene;
    }
    private void Awake()
    {

    }

    public void Init()
    {
        _uiFrame = _defaultUISettings.CreateUIInstance(false);
    }

    private void EnsureScreenLoaded(string id)
    {
        if (_uiFrame != null && !_uiFrame.IsScreenRegistered(id))
        {
            var prefab = _defaultUISettings.GetPrefabByScreenId(id);
            if (prefab != null)
            {
                var screenInstance = _objectResolver.Instantiate(prefab);
                var screenController = screenInstance.GetComponent<IUIScreenController>();
                if (screenController != null)
                {
                    _uiFrame.RegisterScreen(id, screenController, screenInstance.transform);
                    if (screenInstance.activeSelf)
                    {
                        screenInstance.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError($"[UIManager] Prefab {id} missing IUIScreenController.");
                }
            }
            else
            {
                Debug.LogError($"[UIManager] Could not find Screen Prefab for id: {id}");
            }
        }
    }

    public void OpenWindowScene(string id)
    {
        EnsureScreenLoaded(id);
        _uiFrame.OpenWindow(id);
    }

    public void CloseWindowScene(string id)
    {
        _uiFrame.CloseWindow(id);
    }

    public void ShowPanel(string id)
    {
        EnsureScreenLoaded(id);
        _uiFrame.ShowPanel(id);
    }
    public void HidePanel(string id)
    {
        _uiFrame.HidePanel(id);
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
                EnsureScreenLoaded(id);
                _uiFrame.OpenWindow(id);
                break;
            case ScreenIds.GameSettingsScene:
                EnsureScreenLoaded(id);
                _uiFrame.OpenWindow(id);
                break;
            case ScreenIds.PopupConfirm:
                Action cancle = () => { EnsureScreenLoaded(ScreenIds.PanelStartGame); _uiFrame.ShowPanel(ScreenIds.PanelStartGame); };
                Action confirm = () => {
                    Application.Quit();
                };
                string message = LocalizationManager.Instance.GetLocalizedValue("UI_QUIT_QUESTION");
                ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties("Remind", message, "Confirm", "Cancel", confirm, cancle);
                EnsureScreenLoaded(id);
                _uiFrame.OpenWindow(id, popupProps);
                break;
            default:
                EnsureScreenLoaded(id);
                _uiFrame.OpenWindow(id);
                break;
        }
    }

    public void ShowPopupConfirmSettings(PopupSettingProperties popup)
    {
        EnsureScreenLoaded(ScreenIds.PopupConfirmSettings);
        _uiFrame.OpenWindow(ScreenIds.PopupConfirmSettings, popup);
    }

    public void ToggleLoadingScene(bool isOn)
    {
        if (isOn)
            OpenWindowScene(ScreenIds.LoadingScene);
        else
            CloseWindowScene(ScreenIds.LoadingScene);
    }
}
