using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


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

    private ItemType currentItemType = ItemType.None;

    private List<GameObject> weapons = new();
    private List<GameObject> items = new();
    //private List<GameObject> matterials = new();
    private List<GameObject> armors = new();
    private List<GameObject> shards = new();

    private Dictionary<ItemType, List<GameObject>> materials_dic = new();

    private Dictionary<ItemType, List<GameObject>> dictionaryObject = new();

    private ItemCardInfoUI itemCard;

    private InventoryManager _inventoryManager;
    private GameDataBase _gameDataBase;

    private void Awake()
    {
        itemCard = iteamCardInfo.GetComponent<ItemCardInfoUI>();
    }
    private void Start()
    {
        OnShowAllItemInInventory(ItemType.Item);
    }
    private void OnEnable()
    {
        UIEvent.OnSelectToggleInventoryTap += OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem += OnClickItemUI;
        //_inventoryManager.OnInventoryChanged += RefreshData;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleInventoryTap -= OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem -= OnClickItemUI;
        //_inventoryManager.OnInventoryChanged -= RefreshData;
    }
    public void Init(InventoryManager inventoryManager, GameDataBase gameDataBase)
    {
        _inventoryManager = inventoryManager;

        _gameDataBase = gameDataBase;

        RefreshData(); // Gọi lần đầu để vẽ UI

        OnShowAllItemInInventory(ItemType.Item);
    }

    private void RefreshData()
    {
        ClearAllOldUI();

        // 2. VẼ WEAPONS
        foreach (var item in _inventoryManager.Weapons)
        {
            var obj = Instantiate(prefabWeapon, content.transform);
            var weaponConfig = _gameDataBase.GetItemConfig(item.TemplateID); 
            {
                obj.GetComponent<WeaponUI>().Init(item.UUID, weaponConfig.Rarity, 
                    weaponConfig.Icon, weaponConfig.IconBG, item.CurrentLevel, item.CurrentUpgrade);
                obj.SetActive(false);
                weapons.Add(obj);
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
                else if (item.Type == ItemType.Gemstone || item.Type == ItemType.Exp)
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

    public void OnShowAllItemInInventory(ItemType type)
    {
        if (currentItemType == type) return;

        DeActiveAllObjectInContent();
        currentItemType = type;

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

        weaponCardInfo.SetActive(type == ItemType.Weapon);
        iteamCardInfo.SetActive(type == ItemType.Item || type == ItemType.Material || type == ItemType.Shard || type == ItemType.Exp);
        armorCardInfo.SetActive(type == ItemType.Armor);

        // 🌟 Lấy món đồ đầu tiên một cách an toàn
        InventoryItemUI itemUI = listItems[0].GetComponent<InventoryItemUI>();
        itemUI.OnSwitchStatusBoder(true);
        UIEvent.OnSelectInventoryItem?.Invoke(itemUI.ID);

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }


    public void OnClickItemUI(string id)
    {
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
                obj.SetActive(false);
            }
        }
    }

    private void ClearAllOldUI()
    {
        foreach (var list in dictionaryObject.Values)
        {
            foreach (var obj in list) Destroy(obj);
            list.Clear();
        }
        dictionaryObject.Clear();
    }
}
 