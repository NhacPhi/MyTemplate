using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

[Serializable]
public class GachaSelectTargetProperties : WindowProperties
{
    public readonly string title;
    public readonly string bannerId;
    public readonly List<string> selectableItemIds;
    public readonly Action<string> onConfirmed;

    public GachaSelectTargetProperties(string title, string bannerId, List<string> selectableItemIds, Action<string> onConfirmed = null)
    {
        this.title = title;
        this.bannerId = bannerId;
        this.selectableItemIds = selectableItemIds;
        this.onConfirmed = onConfirmed;
    }
}

public class PopupGachaSelectTarget : WindowController<GachaSelectTargetProperties>
{
    [SerializeField] private Button btnClose;
    
    [Header("Current Target UI")]
    [SerializeField] private GachaTargetSlotUI currentTargetSlot;

    [Header("Grid Setup (Content of ScrollView)")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GachaItemCardUI itemCardPrefab;

    [Inject] private GameDataBase db;
    [Inject] private GachaRuntimeManager gachaRuntimeManager;

    private string _currentSelectedId;
    private string _bannerId;
    private List<GachaItemCardUI> _spawnedCards = new List<GachaItemCardUI>();

    private void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(UI_Close);

        if (currentTargetSlot != null)
        {
            currentTargetSlot.OnSlotClicked += OnCurrentTargetClicked;
        }
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();

        _bannerId = Properties.bannerId;
        
        // Lấy target hiện tại từ dữ liệu runtime
        var runtimeData = gachaRuntimeManager.GetRuntimeData(_bannerId);
        _currentSelectedId = runtimeData.SelectedTargetId;

        if (gridParent == null || itemCardPrefab == null) return;

        // Xóa các card cũ
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        _spawnedCards.Clear();

        // Khởi tạo các card mới
        if (Properties.selectableItemIds != null)
        {
            foreach (var itemId in Properties.selectableItemIds)
            {
                var card = Instantiate(itemCardPrefab, gridParent);
                var itemConfig = db.GetItemConfig(itemId);
                
                if (itemConfig != null)
                {
                    card.Setup(_bannerId, itemId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                }
                
                card.OnCardClicked += OnGridItemClicked;
                _spawnedCards.Add(card);
            }
        }

        RefreshCurrentTargetUI();
        RefreshGridUI();
    }

    private void RefreshCurrentTargetUI()
    {
        if (currentTargetSlot == null) return;

        if (string.IsNullOrEmpty(_currentSelectedId))
        {
            currentTargetSlot.SetupEmpty(_bannerId);
        }
        else
        {
            var itemConfig = db.GetItemConfig(_currentSelectedId);
            if (itemConfig != null)
            {
                currentTargetSlot.SetupFilled(_bannerId, _currentSelectedId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
            }
            else
            {
                currentTargetSlot.SetupEmpty(_bannerId);
            }
        }
    }

    private void RefreshGridUI()
    {
        foreach (var card in _spawnedCards)
        {
            if (card != null)
            {
                // Thay vì ẩn/hiện, gọi hàm ToggleSelected để bật/tắt đánh dấu
                card.ToggleSelected(card.ID == _currentSelectedId);
            }
        }
    }

    private void OnCurrentTargetClicked(string bannerId, string itemId)
    {
        // Click vào target slot phía trên thì remove target
        _currentSelectedId = string.Empty;
        RefreshCurrentTargetUI();
        RefreshGridUI();
    }

    private void OnGridItemClicked(string bannerId, string clickedItemId)
    {
        // Click vào item trong danh sách thì set target
        _currentSelectedId = clickedItemId;
        RefreshCurrentTargetUI();
        RefreshGridUI();
    }

    public override void UI_Close()
    {
        if (Properties != null && Properties.onConfirmed != null)
        {
            Properties.onConfirmed.Invoke(_currentSelectedId);
        }
        base.UI_Close();
    }
}
