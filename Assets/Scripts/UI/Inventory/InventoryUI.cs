using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject prefabItem;
    [SerializeField] private GameObject prefabArmor;
    [SerializeField] private GameObject prefabWeapon;
    [SerializeField] private GameObject prefabShare;

    [SerializeField] private GameObject content;

    [SerializeField] private GameObject weaponCardInfo;
    [SerializeField] private GameObject iteamCardInfo;
    [SerializeField] private GameObject armorCardInfo;

    [Header("Recall References")]
    [SerializeField] private Button btnRecall;
    [SerializeField] private GameObject objRecall;
    [SerializeField] private Button btnCloseRecall;
    [SerializeField] private Button btnConfirmRecall;
    [SerializeField] private TextMeshProUGUI txtSelectedCount;
    [SerializeField] private TextMeshProUGUI txtTotalRelicEssence;

    private ItemType currentItemType = ItemType.None;
    private bool isRecallMode = false;
    private HashSet<string> selectedRecallWeaponUUIDs = new HashSet<string>();

    private List<GameObject> weapons = new();
    private List<WeaponUI> weaponUIs = new();
    private List<GameObject> items = new();
    //private List<GameObject> matterials = new();
    private List<GameObject> armors = new();
    private List<GameObject> shards = new();

    private Dictionary<ItemType, List<GameObject>> materials_dic = new();

    private Dictionary<ItemType, List<GameObject>> dictionaryObject = new();

    private ItemCardInfoUI itemCard;

    private InventoryManager _inventoryManager;
    private GameDataBase _gameDataBase;
    private ForgeManager _forgeManager;
    private UIManager _uiManager;

    private void Awake()
    {
        itemCard = iteamCardInfo.GetComponent<ItemCardInfoUI>();
        if (btnRecall != null) btnRecall.onClick.AddListener(OnToggleRecall);
        if (btnCloseRecall != null) btnCloseRecall.onClick.AddListener(() => SetRecallMode(false));
        if (btnConfirmRecall != null) btnConfirmRecall.onClick.AddListener(OnConfirmRecall);
    }
    private void Start()
    {
        OnShowAllItemInInventory(ItemType.Item);
    }
    private void OnEnable()
    {
        UIEvent.OnSelectToggleInventoryTap += OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem += OnClickItemUI;

        UIEvent.OnWeaponUpgraded += HandleWeaponChange;
        UIEvent.OnArmorUpgraded += HandleArmorChange;
        UIEvent.OnItemChanged += HandleItemChanged;
        
        UIEvent.OnInventoryChanged += RefreshData;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleInventoryTap -= OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem -= OnClickItemUI;

        UIEvent.OnWeaponUpgraded -= HandleWeaponChange;
        UIEvent.OnArmorUpgraded -= HandleArmorChange;
        UIEvent.OnItemChanged -= HandleItemChanged;
        
        UIEvent.OnInventoryChanged -= RefreshData;
    }
    public void Init(InventoryManager inventoryManager, GameDataBase gameDataBase, ForgeManager forgeManager = null, UIManager uiManager = null)
    {
        _inventoryManager = inventoryManager;
        _gameDataBase = gameDataBase;
        if (forgeManager != null) _forgeManager = forgeManager;
        if (uiManager != null) _uiManager = uiManager;

        RefreshData(); // Gọi lần đầu để vẽ UI và tự động hiển thị tab hiện tại
    }

    private void RefreshData()
    {
        if (_inventoryManager == null || _gameDataBase == null) return;
        ClearAllOldUI();

        // 2. VẼ WEAPONS
        weaponUIs.Clear();
        if (_inventoryManager.Weapons != null)
        {
            foreach (var item in _inventoryManager.Weapons)
            {
                var weaponConfig = _gameDataBase.GetItemConfig(item.TemplateID); 
                if (weaponConfig != null)
                {
                    var obj = Instantiate(prefabWeapon, content.transform);
                    var weaponUI = obj.GetComponent<WeaponUI>();
                    bool isEquipped = !string.IsNullOrEmpty(item.Equip);
                    bool isUnselectable = isEquipped || weaponConfig.Rarity == Rare.Legendary;
                    weaponUI.Init(item.UUID, weaponConfig.Rarity, 
                        weaponConfig.Icon, weaponConfig.IconBG, item.CurrentLevel, item.CurrentUpgrade, isUnselectable);
                    weaponUI.OnRecallSelectionToggled = OnWeaponRecallSelectionChanged;
                    obj.SetActive(false);
                    weapons.Add(obj);
                    weaponUIs.Add(weaponUI);
                }
            }
        }
        dictionaryObject[ItemType.Weapon] = weapons;

        // 3. VẼ ITEMS
        foreach (var item in _inventoryManager.Items) // Đã đổi sang Dictionary theo bài trước
        {
            var obj = Instantiate(prefabItem, content.transform);
            var itemConfig = _gameDataBase.GetItemConfig(item.ID);
            if (itemConfig != null)
            {
                obj.GetComponent<ItemUI>().Init(item.ID, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG, item.Quantity);
                obj.SetActive(false);

                if (item.Type == ItemType.Food) items.Add(obj);
                else if (item.Type == ItemType.Gemstone || item.Type == ItemType.Exp || item.Type == ItemType.Material)
                {
                    if (!materials_dic.ContainsKey(item.Type))
                    {
                        materials_dic[item.Type] = new List<GameObject>();
                    }
                    materials_dic[item.Type].Add(obj);

                    //matterials.Add(obj);
                }
                else if (item.Type == ItemType.Shard) shards.Add(obj);
            }
        }
        dictionaryObject[ItemType.Item] = items;
        dictionaryObject[ItemType.Material] = materials_dic.Values.SelectMany(list => list).ToList();
        dictionaryObject[ItemType.Shard] = shards;

        // 4. VẼ ARMORS
        foreach (var item in _inventoryManager.Armors)
        {
            var obj = Instantiate(prefabArmor, content.transform);
            var armorConfig = _gameDataBase.GetItemConfig(item.TemplateID);
            if (armorConfig != null)
            {
                obj.GetComponent<ArmorItemUI>().Init(item.UUID, item.Rare, armorConfig.Icon, 
                    _gameDataBase.GetBGItemByRare(item.Rare), item.Level);
                obj.SetActive(false);
                armors.Add(obj);
            }
        }
        dictionaryObject[ItemType.Armor] = armors;

        // Nếu đang ở tab nào thì bật hiển thị lại tab đó
        ItemType tempType = currentItemType == ItemType.None ? ItemType.Item : currentItemType;
        currentItemType = ItemType.None; // Reset để hàm OnShow chịu chạy
        OnShowAllItemInInventory(tempType);
    }

    public void ResetState()
    {
        currentItemType = ItemType.None;
    }

    public void OnShowAllItemInInventory(ItemType type)
    {
        if (currentItemType == type) return;

        if (type != ItemType.Weapon && isRecallMode)
        {
            SetRecallMode(false);
        }

        DeActiveAllObjectInContent();
        currentItemType = type;

        if (btnRecall != null)
        {
            btnRecall.gameObject.SetActive(type == ItemType.Weapon);
        }

        List<GameObject> listItems = dictionaryObject.GetValueOrDefault(type);
        if (listItems == null || listItems.Count == 0)
        {
            // 🌟 NẾU KHÔNG CÓ ĐỒ: Ẩn Card Info đi và dừng lại, CHỐNG CRASH!
            weaponCardInfo.SetActive(false);
            iteamCardInfo.SetActive(false);
            armorCardInfo.SetActive(false);
            return;
        }

        for (int i = 0; i < listItems.Count; i++)
        {
            var item = listItems[i];
            if (item != null)
            {
                item.SetActive(true);
                // Ép vị trí của object trên Hierarchy giống hệt vị trí của nó trong List
                item.transform.SetSiblingIndex(i);
            }
        }

        weaponCardInfo.SetActive(!isRecallMode && type == ItemType.Weapon);
        iteamCardInfo.SetActive(type == ItemType.Item || type == ItemType.Material || type == ItemType.Shard || type == ItemType.Exp);
        armorCardInfo.SetActive(type == ItemType.Armor);

        // 🌟 Lấy món đồ đầu tiên một cách an toàn nếu không trong chế độ recall
        if (!isRecallMode)
        {
            InventoryItemUI itemUI = listItems[0].GetComponent<InventoryItemUI>();
            itemUI.OnSwitchStatusBoder(true);
            UIEvent.OnSelectInventoryItem?.Invoke(itemUI.ID);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }


    public void OnClickItemUI(string id)
    {
        if (isRecallMode && currentItemType == ItemType.Weapon) return;

        List<GameObject> listItem = dictionaryObject.GetValueOrDefault(currentItemType);
        if (listItem == null) return;
        foreach (var item in listItem)
        {
            if (item == null) continue; 
            var obj = item.GetComponent<InventoryItemUI>();
            obj.OnSwitchStatusBoder(false);
            if (obj.ID == id)
            {
                obj.OnSwitchStatusBoder(true);
            }
        }
    }

    private void DeActiveAllObjectInContent()
    {
        foreach (var list in dictionaryObject.Values)
        {
            foreach (var obj in list)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void ClearAllOldUI()
    {
        foreach (var list in dictionaryObject.Values)
        {
            foreach (var obj in list) 
            {
                if (obj != null) Destroy(obj);
            }
        }
        dictionaryObject.Clear();
        weapons.Clear();
        weaponUIs.Clear();
        items.Clear();
        armors.Clear();
        shards.Clear();
        materials_dic.Clear();
    }

    /// <summary>
    /// Xử lý khi sử dụng/bán/nhận thêm Item (Food, Gem, Material...) làm thay đổi số lượng
    /// </summary>
    private void HandleItemChanged(string id)
    {
        // Kiểm tra xem item này còn tồn tại trong kho không, hay đã dùng hết (số lượng = 0)
        var itemSave = _inventoryManager.Items.FirstOrDefault(i => i.ID == id);

        if (itemSave == null || itemSave.Quantity <= 0)
        {
            // Nếu dùng hết sạch => Item đó biến mất khỏi kho => Bắt buộc phải vẽ lại toàn bộ UI
            RefreshData();
            return;
        }

        // Nếu vẫn còn, chỉ cần update lại con số số lượng trên UI
        List<GameObject> currentList = dictionaryObject.GetValueOrDefault(currentItemType);
        if (currentList != null)
        {
            foreach (var go in currentList)
            {
                var uiComp = go.GetComponent<InventoryItemUI>();
                if (uiComp != null && uiComp.ID == id)
                {
                    var config = _gameDataBase.GetItemConfig(id);
                    // Gọi lại hàm Init để nó cập nhật Text số lượng
                    go.GetComponent<ItemUI>().Init(id, config.Rarity, config.Icon, config.IconBG, itemSave.Quantity);
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Xử lý khi Giáp thay đổi trạng thái (Equip, Unequip, Roll Substat, Bị xóa)
    /// </summary>
    /// 
    private void HandleArmorChange(string id)
    {
        // 1. Kiểm tra xem giáp còn trong kho không
        var armorSave = _inventoryManager.Armors.FirstOrDefault(a => a.UUID == id);

        if (armorSave == null)
        {
            RefreshData(); // Đã bị bán/phân rã
            return;
        }

        // 2. Cập nhật lại UI ô icon
        if (currentItemType == ItemType.Armor)
        {
            List<GameObject> currentList = dictionaryObject.GetValueOrDefault(ItemType.Armor);
            if (currentList != null)
            {
                foreach (var go in currentList)
                {
                    var uiComp = go.GetComponent<InventoryItemUI>();
                    if (uiComp != null && uiComp.ID == id)
                    {
                        var config = _gameDataBase.GetItemConfig(armorSave.TemplateID);
                        // Gọi Init để reset lại hình ảnh/level
                        go.GetComponent<ArmorItemUI>().Init(id, armorSave.Rare, config.Icon, _gameDataBase.GetBGItemByRare(armorSave.Rare), armorSave.Level);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Xử lý khi Vũ khí thay đổi trạng thái (Equip, Unequip, Bị xóa do Salvage/Làm phôi)
    /// </summary>
    private void HandleWeaponChange(string id)
    {
        // 1. Kiểm tra xem vũ khí còn trong kho không
        var weaponSave = _inventoryManager.Weapons.FirstOrDefault(w => w.UUID == id);

        if (weaponSave == null)
        {
            // Nếu bị null -> Vũ khí đã bị xóa (phân rã hoặc đập làm nguyên liệu)
            // Bắt buộc phải RefreshData để xóa ô UI đó đi
            RefreshData();
            return;
        }

        // 2. Nếu vẫn còn, cập nhật lại UI của chính ô đó (VD: Hiện icon chữ "E" đang mặc)
        if (currentItemType == ItemType.Weapon)
        {
            List<GameObject> currentList = dictionaryObject.GetValueOrDefault(ItemType.Weapon);
            if (currentList != null)
            {
                foreach (var go in currentList)
                {
                    var uiComp = go.GetComponent<WeaponUI>();
                    if (uiComp != null && uiComp.ID == id)
                    {
                        var config = _gameDataBase.GetItemConfig(weaponSave.TemplateID);
                        bool isEquipped = !string.IsNullOrEmpty(weaponSave.Equip);
                        bool isUnselectable = isEquipped || (config != null && config.Rarity == Rare.Legendary);
                        uiComp.Init(id, config.Rarity, config.Icon, config.IconBG, weaponSave.CurrentLevel, weaponSave.CurrentUpgrade, isUnselectable);
                        if (isRecallMode)
                        {
                            uiComp.SetRecallMode(isRecallMode, isUnselectable);
                        }
                        break;
                    }
                }
            }
        }
    }

    public void OnToggleRecall()
    {
        SetRecallMode(!isRecallMode);
    }

    public void SetRecallMode(bool active)
    {
        isRecallMode = active;
        selectedRecallWeaponUUIDs.Clear();

        if (objRecall != null && objRecall.activeSelf != isRecallMode)
        {
            objRecall.SetActive(isRecallMode);
        }

        if (weaponCardInfo != null && weaponCardInfo.activeSelf != (!isRecallMode && currentItemType == ItemType.Weapon))
        {
            weaponCardInfo.SetActive(!isRecallMode && currentItemType == ItemType.Weapon);
        }

        // Cập nhật tất cả Weapon UI cực nhanh (sử dụng cache weaponUIs)
        for (int i = 0; i < weaponUIs.Count; i++)
        {
            var wUI = weaponUIs[i];
            if (wUI != null)
            {
                wUI.SetRecallMode(isRecallMode);
                wUI.OnSwitchStatusBoder(false);
            }
        }

        UpdateRecallUIStats();

        // Nếu thoát chế độ Recall và đang ở tab Weapon, chọn lại món đầu tiên
        if (!isRecallMode && currentItemType == ItemType.Weapon && weaponUIs.Count > 0)
        {
            var firstUI = weaponUIs[0];
            if (firstUI != null)
            {
                firstUI.OnSwitchStatusBoder(true);
                UIEvent.OnSelectInventoryItem?.Invoke(firstUI.ID);
            }
        }
    }

    private void OnWeaponRecallSelectionChanged(string uuid, bool isSelected)
    {
        if (isSelected)
            selectedRecallWeaponUUIDs.Add(uuid);
        else
            selectedRecallWeaponUUIDs.Remove(uuid);

        UpdateRecallUIStats();
    }

    public int GetTotalSelectableWeaponsCount()
    {
        int total = 0;
        for (int i = 0; i < weaponUIs.Count; i++)
        {
            if (weaponUIs[i] != null && !weaponUIs[i].IsUnselectable)
            {
                total++;
            }
        }
        return total;
    }

    private void UpdateRecallUIStats()
    {
        int selectedCount = selectedRecallWeaponUUIDs.Count;
        int totalSelectable = 0;
        int totalEssence = 0;

        for (int i = 0; i < weaponUIs.Count; i++)
        {
            var wUI = weaponUIs[i];
            if (wUI != null && !wUI.IsUnselectable)
            {
                totalSelectable++;
                if (selectedRecallWeaponUUIDs.Contains(wUI.ID))
                {
                    totalEssence += wUI.SalvageEssenceValue;
                }
            }
        }

        if (txtSelectedCount != null)
        {
            txtSelectedCount.text = $"{selectedCount}/{totalSelectable}";
        }

        if (txtTotalRelicEssence != null)
        {
            txtTotalRelicEssence.text = Utility.FormatCurrency(totalEssence);
        }
    }

    private void OnConfirmRecall()
    {
        if (selectedRecallWeaponUUIDs == null || selectedRecallWeaponUUIDs.Count == 0) return;

        int totalEssence = 0;
        for (int i = 0; i < weaponUIs.Count; i++)
        {
            var wUI = weaponUIs[i];
            if (wUI != null && selectedRecallWeaponUUIDs.Contains(wUI.ID))
            {
                totalEssence += wUI.SalvageEssenceValue;
            }
        }

        var uuidsToSalvage = new List<string>(selectedRecallWeaponUUIDs);
        int count = 0;
        if (_forgeManager != null)
        {
            count = _forgeManager.SalvageWeapons(uuidsToSalvage);
        }
        else if (_inventoryManager != null)
        {
            _inventoryManager.RemoveWeapons(uuidsToSalvage);
            count = uuidsToSalvage.Count;
        }

        if (count > 0)
        {
            selectedRecallWeaponUUIDs.Clear();
            SetRecallMode(false);

            if (totalEssence > 0 && _uiManager != null)
            {
                var rewards = new List<RewardItemData>
                {
                    new RewardItemData("RelicEssence", totalEssence)
                };
                _uiManager.ShowReceiveItemPopup(new ReceiveItemProperties(rewards));
            }
        }
    }
}
 