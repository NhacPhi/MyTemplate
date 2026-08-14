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
        UIEvent.OnToggleLoadingScene += ToggleLoadingScene;
    }

    private void OnDisable()
    {
        UIEvent.OnToggleLoadingScene -= ToggleLoadingScene;
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

    public void ShowNotification(string message, string title = null, Action confirmAction = null)
    {
        string popupTitle = string.IsNullOrEmpty(title) 
            ? LocalizationManager.Instance.GetLocalizedValue("UI_REMIND") 
            : title;
            
        string confirmBtn = LocalizationManager.Instance.GetLocalizedValue("UI_CONFIRM");
        if (string.IsNullOrEmpty(confirmBtn) || confirmBtn == "UI_CONFIRM")
        {
            confirmBtn = "OK";
        }

        NotificationPopupProperties notificationProps = new NotificationPopupProperties(
            popupTitle,
            message,
            confirmBtn,
            confirmAction
        );
        OpenWindowScene(ScreenIds.PopupNotification, notificationProps);
    }

    public void ShowNotEnoughResourceNotification(CurrencyType type, Action confirmAction = null)
    {
        ShowNotEnoughResourceNotification(type.ToString(), confirmAction);
    }

    public void ShowNotEnoughResourceNotification(string resourceID, Action confirmAction = null)
    {
        string resourceName = LocalizationManager.Instance.GetLocalizedValue(resourceID);
        if (string.IsNullOrEmpty(resourceName))
        {
            resourceName = resourceID;
        }

        string msg = LocalizationManager.Instance.GetLocalizedFormat("msg_not_enough_resource", "resource_name", resourceName);
        if (string.IsNullOrEmpty(msg) || msg == "msg_not_enough_resource")
        {
            msg = $"Không đủ {resourceName}!";
        }

        ShowNotification(msg, null, confirmAction);
    }


    public void PreloadScreen(string id)
    {
        EnsureScreenLoaded(id);
    }

    public void OpenWindowScene(string id)
    {
        Debug.Log($"[TransitionLog] UIManager: OpenWindowScene requested for {id}");
        EnsureScreenLoaded(id);
        var currentWindow = _uiFrame != null ? _uiFrame.GetCurrentWindow() : null;
        if (currentWindow != null && currentWindow.ScreenId == id)
        {
            Debug.Log($"[UIManager] Window {id} is already open and active. Skipping duplicate open call.");
            return;
        }
        _uiFrame.OpenWindow(id);
    }

    public void OpenWindowScene<T>(string id, T properties) where T : WindowProperties
    {
        Debug.Log($"[TransitionLog] UIManager: OpenWindowScene<T> requested for {id}");
        EnsureScreenLoaded(id);
        var currentWindow = _uiFrame != null ? _uiFrame.GetCurrentWindow() : null;
        if (currentWindow != null && currentWindow.ScreenId == id)
        {
            Debug.Log($"[UIManager] Window {id} is already open and active. Skipping duplicate open call.");
            return;
        }
        _uiFrame.OpenWindow(id, properties);
    }

    public void CloseWindowScene(string id)
    {
        Debug.Log($"[TransitionLog] UIManager: CloseWindowScene requested for {id}");
        _uiFrame.CloseWindow(id);
    }

    public void ShowPanel(string id)
    {
        Debug.Log($"[TransitionLog] UIManager: ShowPanel requested for {id}");
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
        
        Debug.Log($"[TransitionLog] UIManager: ToggleLoadingScene - isOn = {isOn}, sceneId = {sceneId}, isFirstLoad = {_isFirstLoad}");

        if (isOn)
        {
            OpenWindowScene(sceneId);
        }
        else
        {
            // Không tự đóng LaunchLoadingScene bằng code ở đây.
            // Hãy để UIManager.OpenWindowScene(StartGameScene) tự động thay thế và đóng 
            // LaunchLoadingScene theo luồng chuyển cảnh (transition) mặc định của UIFramework.
            // Việc này giúp tránh hoàn toàn lỗi "Hide requested..." và loại bỏ frame trống (blank screen).
            if (!_isFirstLoad)
            {
                var currentWindow = _uiFrame.GetCurrentWindow();
                if (currentWindow != null && currentWindow.ScreenId == sceneId)
                {
                    CloseWindowScene(sceneId);
                }
            }
            _isFirstLoad = false;
        }
    }

    public IWindowController GetCurrentWindow()
    {
        return _uiFrame != null ? _uiFrame.GetCurrentWindow() : null;
    }

    public bool IsInMainGameplay()
    {
        if (_uiFrame == null) return true;
        var currentWindow = _uiFrame.GetCurrentWindow();
        if (currentWindow == null) return true;

        string id = currentWindow.ScreenId;
        return id == ScreenIds.GamePlayScene || id == ScreenIds.GamePlayPanel;
    }
}
