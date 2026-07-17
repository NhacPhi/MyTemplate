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
        if (_uiFrame != null) return;
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

    public void ShowQuitPopup(Action confirmAction, Action cancelAction = null)
    {
        string title = LocalizationManager.Instance.GetLocalizedValue("UI_REMIND");
        string content = LocalizationManager.Instance.GetLocalizedValue("UI_QUIT_QUESTION");
        string confirmBtn = LocalizationManager.Instance.GetLocalizedValue("UI_CONFIRM");
        string cancelBtn = LocalizationManager.Instance.GetLocalizedValue("UI_CANCEL");
        ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties(
            title,
            content,
            confirmBtn,
            cancelBtn,
            confirmAction,
            cancelAction
        );
        OpenWindowScene(ScreenIds.PopupConfirm, popupProps);
    }


    public void PreloadScreen(string id)
    {
        EnsureScreenLoaded(id);
    }

    public void OpenWindowScene(string id)
    {
        EnsureScreenLoaded(id);
        _uiFrame.OpenWindow(id);
    }

    public void OpenWindowScene<T>(string id, T properties) where T : WindowProperties
    {
        EnsureScreenLoaded(id);
        _uiFrame.OpenWindow(id, properties);
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
                ConfirmationPopupProperties popupProps = new ConfirmationPopupProperties(
                    LocalizationManager.Instance.GetLocalizedValue("UI_REMIND"), 
                    LocalizationManager.Instance.GetLocalizedValue("UI_QUIT_QUESTION"), 
                    LocalizationManager.Instance.GetLocalizedValue("UI_CONFIRM"), 
                    LocalizationManager.Instance.GetLocalizedValue("UI_CANCEL"), 
                    confirm, 
                    cancle
                );
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

    public void ShowShopBuyPopup(ShopBuyPopupProperties popup)
    {
        EnsureScreenLoaded(ScreenIds.PopupShopBuy);
        _uiFrame.OpenWindow(ScreenIds.PopupShopBuy, popup);
    }

    public void ShowReceiveItemPopup(ReceiveItemProperties popup)
    {
        EnsureScreenLoaded(ScreenIds.PopupReceiveItem);
        _uiFrame.OpenWindow(ScreenIds.PopupReceiveItem, popup);
    }

    public void ShowBattleResultPopup(BattleResultProperties popup)
    {
        EnsureScreenLoaded(ScreenIds.PopupBattleResult);
        _uiFrame.OpenWindow(ScreenIds.PopupBattleResult, popup);
    }

    private bool _isFirstLoad = true;

    public void ToggleLoadingScene(bool isOn)
    {
        if (_uiFrame == null) Init();

        string sceneId = _isFirstLoad ? ScreenIds.LaunchLoadingScene : ScreenIds.LoadingSceneToScene;
        
        if (isOn)
        {
            OpenWindowScene(sceneId);
        }
        else
        {
            var currentWindow = _uiFrame.GetCurrentWindow();
            if (currentWindow != null && currentWindow.ScreenId == sceneId)
            {
                CloseWindowScene(sceneId);
            }
            _isFirstLoad = false;
        }
    }
}
