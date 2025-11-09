using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            var weaponConfig = gameDataBase.GetItemConfigByID<WeaponConfig>(ItemType.Weapon,item.ID);
            var weaponSO = gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, item.ID);
            obj.GetComponent<WeaponUI>().Init(item.ID, weaponConfig.Rare, weaponSO.Icon, gameDataBase.GetRareBG(weaponConfig.Rare), item.CurrentLevel, item.CurrentUpgrade);
            obj.SetActive(false);
            weapons.Add(obj);
        }
        dictionaryObject.Add(ItemType.Weapon, weapons);

        foreach (var item in save.Player.Items)
        {
            var obj = Instantiate(prefabItem, content.transform);
            var itemConfig = gameDataBase.GetItemConfigByID<ItemBaseConfig>(item.Type, item.ID);
            var itemSO = gameDataBase.GetItemSOByID<ItemBaseSO>(item.Type, item.ID); 
            if(itemConfig == null || itemSO == null)
            {
                Debug.Log("ItemSO id: " + item.ID);
                continue;
            }
            obj.GetComponent<ItemUI>().Init(item.ID, itemConfig.Rare, itemSO.Icon, gameDataBase.GetRareBG(itemConfig.Rare), item.Quanlity);
            obj.SetActive(false);
            if(item.Type == ItemType.Food)
            {
                items.Add(obj);
            }
            else if(item.Type == ItemType.GemStone || item.Type == ItemType.Exp)
            {
                matterials.Add(obj);
            }
            else if (item.Type == ItemType.Shard)
            {
                obj.GetComponent<ItemUI>().ActiveFragIcon(true);
                shards.Add(obj);
            }
        }
        dictionaryObject.Add(ItemType.Item, items);
        dictionaryObject.Add(ItemType.Material, matterials);
        dictionaryObject.Add(ItemType.Shard, shards);
        itemCard.UpdateItemCardInfor(save.Player.Items[0].ID);

        foreach (var item in save.Player.Armors)
        {
            var obj = Instantiate(prefabArmor, content.transform);
            var armorConfig = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, item.TemplateID);
            var armorSO = gameDataBase.GetItemSOByID<ArmorSO>(ItemType.Armor, item.TemplateID);
            obj.GetComponent<ArmorItemUI>().Init(item.InstanceID, item.Rare, armorSO.Icon, gameDataBase.GetRareBG(item.Rare), item.Level);
            obj.SetActive(false);
            armors.Add(obj);
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
 