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

    private ItemType currentItemType = ItemType.Item;

    private List<GameObject> weapons = new();
    private List<GameObject> items = new();
    private List<GameObject> matterials = new();

    private ItemCardInfoUI itemCard;
    private WeaponCardInfoUI weaponCard;

    private void Awake()
    {
        itemCard = iteamCardInfo.GetComponent<ItemCardInfoUI>();
        weaponCard = weaponCardInfo.GetComponent<WeaponCardInfoUI>();
    }
    private void Start()
    {
        OnShowAllItemInInventory(ItemType.Item);
    }
    private void OnEnable()
    {
        UIEvent.OnSelectToggleTap += OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem += OnClickItemUI;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectToggleTap -= OnShowAllItemInInventory;
        UIEvent.OnSelectInventoryItem -= OnClickItemUI;
    }
    public void Init(SaveSystem save, ItemDataBase itemDataBase)
    {
        foreach (var item in save.Player.Weapons)
        {
            var obj = Instantiate(prefabWeapon, content.transform);
            var weaponConfig = itemDataBase.GetItemConfigByID<WeaponConfig>(ItemType.Weapon,item.ID);
            var weaponSO = itemDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, item.ID);
            obj.GetComponent<WeaponUI>().Init(item.ID, weaponConfig.Rare, weaponSO.Icon, itemDataBase.GetRareBG(weaponConfig.Rare), item.CurrentLevel, item.CurrentUpgrade);
            obj.SetActive(false);
            weapons.Add(obj);
        }

        foreach(var item in save.Player.Items)
        {
            var obj = Instantiate(prefabItem, content.transform);
            var weaponConfig = itemDataBase.GetItemConfigByID<ItemBaseConfig>(item.Type, item.ID);
            var weaponSO = itemDataBase.GetItemSOByID<ItemBaseSO>(item.Type, item.ID);
            if (weaponConfig == null || weaponSO == null)
            {
                Debug.Log("ItemID: " + item.ID);
                continue;
            }    


            obj.GetComponent<ItemUI>().Init(item.ID, weaponConfig.Rare, weaponSO.Icon, itemDataBase.GetRareBG(weaponConfig.Rare), item.Count);
            obj.SetActive(false);
            if(item.Type == ItemType.Food)
            {
                items.Add(obj);
            }
            else if(item.Type == ItemType.GemStone)
            {
                matterials.Add(obj);
            }
        }
        itemCard.UpdateItemCardInfor(save.Player.Items[0].ID);
    }

    public void OnShowAllItemInInventory(ItemType type)
    {
        DeActiveAllObjectInContent();
        switch (type)
        {
            case ItemType.Weapon:
                {
                    if(currentItemType != ItemType.Weapon)
                    {
                        weaponCardInfo.SetActive(true);
                        iteamCardInfo.SetActive(false);
                    }
                    currentItemType = ItemType.Weapon;
 
                    foreach (var item in weapons)
                    {
                        item.gameObject.SetActive(true);
                        //item.transform.SetParent(content.transform, false);
                    }
                    WeaponUI weonUI = weapons[0].gameObject.GetComponent<WeaponUI>();
                    if(weonUI != null)
                    {
                        weonUI.OnSwitchStatusBoder(true);
                        UIEvent.OnSelectInventoryItem?.Invoke(weonUI.ID);
                        //weaponCard.UpdateWeaponCardInfor(ui.ID);
                    }
                }
                break;
            case ItemType.Item:
                {
                    if (currentItemType != ItemType.Item)
                    {
                        weaponCardInfo.SetActive(false);
                        iteamCardInfo.SetActive(true);
                        currentItemType = ItemType.Item;
                    }

                    foreach (var item in items)
                    {
                        item.gameObject.SetActive(true);
                        //item.transform.SetParent(content.transform, false);
                    }
                    ItemUI itemUI = items[0].gameObject.GetComponent<ItemUI>();
                    if (itemUI != null)
                    {
                        UIEvent.OnSelectInventoryItem?.Invoke(itemUI.ID);
                        itemUI.OnSwitchStatusBoder(true);
                        //itemCard.UpdateItemCardInfor(ui.ID);
                    }                 
                }
                break;
            case ItemType.Material:
                if (currentItemType != ItemType.Material)
                {
                    weaponCardInfo.SetActive(false);
                    iteamCardInfo.SetActive(true);
                }
                currentItemType = ItemType.Material;

                foreach (var item in matterials)
                {
                    item.gameObject.SetActive(true);
                    //item.transform.SetParent(content.transform, false);
                }
                ItemUI materialUI = matterials[0].gameObject.GetComponent<ItemUI>();
                if (materialUI != null)
                {
                    materialUI.OnSwitchStatusBoder(true);
                    UIEvent.OnSelectInventoryItem?.Invoke(materialUI.ID);
                    //weaponCard.UpdateWeaponCardInfor(ui.ID);
                }
                break;
        }

        // Force rebuild UI layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void OnClickItemUI(string id)
    {
        switch (currentItemType)
        {
            case ItemType.Weapon:
                {
                    foreach (var item in weapons)
                    {
                        var obj = item.GetComponent<WeaponUI>();
                        obj.OnSwitchStatusBoder(false);
                        if(obj.ID == id)
                        {
                            obj.OnSwitchStatusBoder(true);
                        }
                    }
                }
            break;
            case ItemType.Item:
                {
                    foreach (var item in items)
                    {
                        var obj = item.GetComponent<ItemUI>();
                        obj.OnSwitchStatusBoder(false);
                        if (obj.ID == id)
                        {
                            obj.OnSwitchStatusBoder(true);
                        }
                    }
                }
                break;
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
 