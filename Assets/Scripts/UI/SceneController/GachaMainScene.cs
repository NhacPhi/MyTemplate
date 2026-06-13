using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

[Serializable]
public class BannerSetup
{
    public string bannerId;
    public GachaBannerPanel panel;
    public ToggleBase toggle;
}

public class GachaMainScene : WindowController
{
    [Header("Banner Configurations")]
    [SerializeField] private List<BannerSetup> bannerSetups;

    [Header("Currency Text Fields")]
    [SerializeField] private TextMeshProUGUI txtTicketCount;
    [SerializeField] private TextMeshProUGUI txtJadeCount;

    [Header("Action Buttons")]
    [SerializeField] private Button btnClose;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyManager;
    [Inject] private GameDataBase db;
    [Inject] private GachaRuntimeManager gachaRuntimeManager;

    private string _currentBannerId;

    private void Start()
    {
        if (btnClose != null) btnClose.onClick.AddListener(OnCloseClicked);

        // Khởi tạo các Banner Panel
        foreach (var setup in bannerSetups)
        {
            if (setup.panel != null)
            {
                setup.panel.Setup(setup.bannerId);
                setup.panel.OnRequestRoll += HandleRollRequest;
                
                // Đăng ký sự kiện click vào Slot Target
                var slotUI = setup.panel.GetTargetSlotUI();
                if (slotUI != null)
                {
                    slotUI.OnSlotClicked += OpenSelectTargetPopup;
                }
            }

            if (setup.toggle != null && setup.toggle.Toggle != null)
            {
                setup.toggle.Toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn) SwitchBanner(setup.bannerId);
                });
            }
        }

        GachaRollState.OnRequestRollAgain += HandleRollAgainRequest;

        // Bật banner đầu tiên làm mặc định
        if (bannerSetups.Count > 0)
        {
            SwitchBanner(bannerSetups[0].bannerId);
        }
    }

    private void OnDestroy()
    {
        GachaRollState.OnRequestRollAgain -= HandleRollAgainRequest;
        
        foreach (var setup in bannerSetups)
        {
            if (setup.panel != null)
            {
                setup.panel.OnRequestRoll -= HandleRollRequest;
                var slotUI = setup.panel.GetTargetSlotUI();
                if (slotUI != null)
                {
                    slotUI.OnSlotClicked -= OpenSelectTargetPopup;
                }
            }
        }
    }

    private void HandleRollAgainRequest(int count)
    {
        if (uiManager != null)
        {
            uiManager.CloseWindowScene(ScreenIds.GachaResultScene);
        }
        HandleRollRequest(_currentBannerId, count);
    }

    private void OnEnable()
    {
        UpdateCurrencyDisplay();
        UIEvent.OnCurrencyChanged += OnCurrencyChanged;
        RefreshCurrentBannerUI();
    }

    private void OnDisable()
    {
        UIEvent.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(CurrencyType type, int amount)
    {
        UpdateCurrencyDisplay();
    }

    private void UpdateCurrencyDisplay()
    {
        if (currencyManager != null)
        {
            if (txtTicketCount != null)
                txtTicketCount.text = currencyManager.GetQuantityCurrecy(CurrencyType.Ticket).ToString();
            
            if (txtJadeCount != null)
                txtJadeCount.text = currencyManager.GetQuantityCurrecy(CurrencyType.Jade).ToString();
        }
    }

    private void SwitchBanner(string bannerId)
    {
        _currentBannerId = bannerId;

        foreach (var setup in bannerSetups)
        {
            bool isCurrent = (setup.bannerId == bannerId);
            
            if (setup.panel != null)
            {
                setup.panel.gameObject.SetActive(isCurrent);
                if (isCurrent)
                {
                    RefreshBannerPanel(setup.panel);
                }
            }

            if (setup.toggle != null && setup.toggle.Toggle != null && isCurrent && !setup.toggle.Toggle.isOn)
            {
                setup.toggle.ActiveToggle(true);
            }
        }
    }

    private void RefreshCurrentBannerUI()
    {
        if (string.IsNullOrEmpty(_currentBannerId)) return;
        
        var setup = bannerSetups.Find(s => s.bannerId == _currentBannerId);
        if (setup != null && setup.panel != null)
        {
            RefreshBannerPanel(setup.panel);
        }
    }

    private void RefreshBannerPanel(GachaBannerPanel panel)
    {
        var runtimeData = gachaRuntimeManager.GetRuntimeData(panel.BannerId);
        panel.RefreshTargetSlot(runtimeData.SelectedTargetId, db);

        var config = db.GetGachaConfig(panel.BannerId);
        if (config != null)
        {
            int remainingPity = config.PityLimit - runtimeData.PityCount;
            if (remainingPity < 0) remainingPity = 0;
            panel.UpdatePityNote(remainingPity);
        }
    }

    private void OpenSelectTargetPopup(string bannerId, string itemId)
    {
        var config = db.GetGachaConfig(bannerId);
        if (config == null || !config.AllowSelection) return;

        // Lọc danh sách item có thể chọn (IsSelectableTarget == true)
        List<string> selectableIds = new List<string>();
        if (config.Pool != null)
        {
            foreach (var item in config.Pool)
            {
                if (item.IsSelectableTarget)
                {
                    selectableIds.Add(item.ItemId);
                }
            }
        }

        var props = new GachaSelectTargetProperties(
            "Select Target", 
            bannerId,
            selectableIds, 
            (selectedId) => OnTargetSelected(bannerId, selectedId)
        );
        uiManager.OpenWindowScene(ScreenIds.PopupGachaSelectTarget, props);
    }

    private void OnTargetSelected(string bannerId, string targetId)
    {
        gachaRuntimeManager.SetSelectedTarget(bannerId, targetId);
        RefreshCurrentBannerUI();
    }

    [Inject] private GachaManager gachaManager;

    private void HandleRollRequest(string bannerId, int count)
    {
        Debug.Log($"[GachaMainScene] Requested Roll {count}x on {bannerId} banner.");
        
        GachaRollState.LastBannerType = bannerId;
        GachaRollState.LastRollCount = count;

        // Gọi GachaManager xử lý random và lấy kết quả
        if (gachaManager != null)
        {
            var results = gachaManager.RollGacha(bannerId, count);
            GachaRollState.LastResults = results;
        }

        // Transition to GachaCutsceneUI
        if (uiManager != null)
        {
            UI_Close();
            uiManager.OpenWindowScene(ScreenIds.GachaCutsceneScene);
        }
    }

    private void ShowInsufficientCurrencyPopup()
    {
        if (uiManager != null)
        {
            Action confirmAction = () => {
                uiManager.OpenWindowScene(ScreenIds.ShopScene);
            };
            
            Action cancelAction = () => {
                uiManager.OpenWindowScene(ScreenIds.GachaMainScene);
            };

            var popupProps = new ConfirmationPopupProperties(
                "Remind", 
                "Insufficient tickets or jades to roll. Go to Shop?", 
                "Go to Shop", 
                "Cancel", 
                confirmAction, 
                cancelAction
            );

            uiManager.OpenWindowScene(ScreenIds.PopupConfirm);
        }
    }

    private void OnCloseClicked()
    {
        UI_Close();
    }
}

public class GachaItemResult
{
    public string itemId;
    public string itemName;
    public Rare rarity; // Sử dụng chung enum Rare của game thay vì GachaRarity
    public bool isCharacter;
}

public static class GachaRollState
{
    public static List<GachaItemResult> LastResults = new List<GachaItemResult>();
    public static Rare HighestRarity = Rare.Common;
    public static int LastRollCount = 1;
    public static string LastBannerType = "banner_char_01";
    public static System.Action<int> OnRequestRollAgain;
}
