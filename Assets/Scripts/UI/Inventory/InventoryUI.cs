using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
    private List<GameObject> matterials = new();
    private List<GameObject> armors = new();
    private List<GameObject> shards = new();

    private Dictionary<ItemType, List<GameObject>> dictionaryObject = new();

    private ItemCardInfoUI itemCard;

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
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleInventoryTap -= OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem -= OnClickItemUI;
    }
    public void Init(SaveSystem save, GameDataBase gameDataBase)
    {
        foreach (var item in save.Player.Weapons)
        {
            var obj = Instantiate(prefabWeapon, content.transform);
            var weaponConfig = gameDataBase.GetItemConfig(item.ID);
            if(weaponConfig != null)
            {
                obj.GetComponent<WeaponUI>().Init(item.ID, weaponConfig.Rarity, weaponConfig.Icon, weaponConfig.IconBG, item.CurrentLevel, item.CurrentUpgrade);
                obj.SetActive(false);
                weapons.Add(obj);
            }
            
        }
        dictionaryObject.Add(ItemType.Weapon, weapons);

        foreach (var item in save.Player.Items)
        {
            var obj = Instantiate(prefabItem, content.transform);
            var itemConfig = gameDataBase.GetItemConfig(item.ID);
            if (itemConfig != null)
            {
                obj.GetComponent<ItemUI>().Init(item.ID, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG, item.Quantity);
                obj.SetActive(false);
                if (item.Type == ItemType.Food)
                {
                    items.Add(obj);
                }
                else if (item.Type == ItemType.Gemstone || item.Type == ItemType.Exp)
                {
                    matterials.Add(obj);
                }
                else if (item.Type == ItemType.Shard)
                {
                    obj.GetComponent<ItemUI>().ActiveFragIcon(true);
                    shards.Add(obj);
                }
            }

        }
        dictionaryObject.Add(ItemType.Item, items);
        dictionaryObject.Add(ItemType.Material, matterials);
        dictionaryObject.Add(ItemType.Shard, shards);
        itemCard.UpdateItemCardInfor(save.Player.Items[0].ID);

        foreach (var item in save.Player.Armors)
        {
            var obj = Instantiate(prefabArmor, content.transform);
            var armorConfig = gameDataBase.GetItemConfig(item.TemplateID);
            if(armorConfig != null)
            {
                obj.GetComponent<ArmorItemUI>().Init(item.InstanceID, item.Rare, armorConfig.Icon, gameDataBase.GetBGItemByRare(item.Rare), item.Level);
                obj.SetActive(false);
                armors.Add(obj);
            }
        }
        dictionaryObject.Add(ItemType.Armor, armors);
    }

    public void OnShowAllItemInInventory(ItemType type)
    {
        if (currentItemType == type) return;
        DeActiveAllObjectInContent();
        List<GameObject> listItems = dictionaryObject.GetValueOrDefault(type);
        currentItemType = type;
        if (listItems == null) return;
        foreach (var item in listItems)
        {
            item.gameObject.SetActive(true);
        }

        weaponCardInfo.SetActive(type == ItemType.Weapon ? true : false);
        iteamCardInfo.SetActive((type == ItemType.Item || type == ItemType.Material || type == ItemType.Shard || ItemType.Exp == type) ? true : false);
        armorCardInfo.SetActive(type == ItemType.Armor ? true : false);

        InventoryItemUI itemUI = listItems[0].gameObject.GetComponent<InventoryItemUI>();
        itemUI.OnSwitchStatusBoder(true);
        UIEvent.OnSelectInventoryItem?.Invoke(itemUI.ID);

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void OnClickItemUI(string id)
    {
        List<GameObject> listItem = dictionaryObject.GetValueOrDefault(currentItemType);
        if (listItem == null) return;
        foreach (var item in listItem)
        {
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
        InventoryItemUI[] objects = content.GetComponentsInChildren<InventoryItemUI>();
        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
 