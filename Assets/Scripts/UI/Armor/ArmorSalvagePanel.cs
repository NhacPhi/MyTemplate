using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Panel quản lý việc quy đổi (salvage) armor thành ArmorPrimorite.
/// Hiển thị danh sách armor chưa equipped, cho phép chọn/bỏ chọn,
/// tính tổng primorite nhận được, và nút Thu hồi (bỏ chọn tất cả) / Quy đổi.
/// </summary>
public class ArmorSalvagePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform content;
    [SerializeField] private ArmorSalvageItemUI prefabItem;
    [SerializeField] private TextMeshProUGUI txtTotalPrimorite;

    [Header("Buttons")]
    [SerializeField] private Button btnRecall;

    [Inject] private GameDataBase gameDataBase;
    [Inject] private InventoryManager inventoryManager;
    [Inject] private ForgeManager forgeManager;

    private List<ArmorSalvageItemUI> itemUIs = new List<ArmorSalvageItemUI>();
    private HashSet<string> selectedUUIDs = new HashSet<string>();

    private void Awake()
    {
        if (btnRecall != null) btnRecall.onClick.AddListener(OnBtnSalvageClicked);

        UIEvent.OnArmorUpgraded += OnArmorChanged;
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void OnDestroy()
    {
        UIEvent.OnArmorUpgraded -= OnArmorChanged;
    }

    private void OnArmorChanged(string uuid)
    {
        RefreshList();
    }

    /// <summary>
    /// Refresh toàn bộ danh sách armor khả dụng (chưa equipped)
    /// </summary>
    private void RefreshList()
    {
        selectedUUIDs.Clear();

        var salvageableArmors = inventoryManager.Armors
            .Where(a => string.IsNullOrEmpty(a.Equip))
            .ToList();

        // Đồng bộ UI pool
        for (int i = 0; i < salvageableArmors.Count; i++)
        {
            var armor = salvageableArmors[i];
            ArmorSalvageItemUI itemUI;

            if (i < itemUIs.Count)
            {
                itemUI = itemUIs[i];
                itemUI.gameObject.SetActive(true);
            }
            else
            {
                var obj = Instantiate(prefabItem, content);
                itemUI = obj.GetComponent<ArmorSalvageItemUI>();
                itemUI.OnSelectionChanged += OnItemSelectionChanged;
                itemUIs.Add(itemUI);
            }

            var config = gameDataBase.GetItemConfig(armor.TemplateID);
            //int primoriteValue = Utility.GetArmorPrimoriteFromSalvage(armor.Rare, armor.Level);

            itemUI.Init(
                armor.UUID,
                armor.Rare,
                config.Icon,
                gameDataBase.GetBGItemByRare(armor.Rare),
                armor.Level
            );
        }

        // Ẩn các item UI thừa
        for (int i = salvageableArmors.Count; i < itemUIs.Count; i++)
        {
            itemUIs[i].gameObject.SetActive(false);
        }

        UpdateTotalPrimorite();
    }

    /// <summary>
    /// Callback khi 1 item được chọn/bỏ chọn
    /// </summary>
    private void OnItemSelectionChanged(string uuid, bool isSelected)
    {
        if (isSelected)
            selectedUUIDs.Add(uuid);
        else
            selectedUUIDs.Remove(uuid);

        UpdateTotalPrimorite();
    }

    /// <summary>
    /// Cập nhật tổng ArmorPrimorite sẽ nhận được
    /// </summary>
    private void UpdateTotalPrimorite()
    {
        int total = 0;
        foreach (var uuid in selectedUUIDs)
        {
            total += forgeManager.GetSalvagePrimoriteValue(uuid);
        }

        if (txtTotalPrimorite != null) txtTotalPrimorite.text = Utility.FormatCurrency(total);
    }


    /// <summary>
    /// Nút Quy đổi - salvage tất cả armor đã chọn
    /// </summary>
    private void OnBtnSalvageClicked()
    {
        if (selectedUUIDs.Count == 0) return;

        var uuidsToSalvage = new List<string>(selectedUUIDs);
        int count = forgeManager.SalvageArmors(uuidsToSalvage);

        if (count > 0)
        {
            RefreshList();
        }
    }
}
