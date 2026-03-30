using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class InventorySaveData 
{
    [JsonProperty("currencies")]
    public Dictionary<CurrencyType, int> Currencies;

    [JsonProperty("items")]
    public List<ItemSaveData> Items;

    [JsonProperty("weapons")]
    public List<WeaponSaveData> Weapons;

    [JsonProperty("armors")]
    public List<ArmorSaveData> Armors;

    public WeaponSaveData GetWeapon(string id)
    {
        return Weapons.Find(v => v.UUID == id);
    }

    public ItemSaveData GetItem(string id)
    {
        return Items.Find(v => v.ID == id);
    }

    public ArmorSaveData GetArmor(string id)
    {
        return Armors.Find(v => v.UUID == id);
    }

    public List<ItemSaveData> GetAllItemByType(ItemType type)
    {
        List<ItemSaveData> list = new List<ItemSaveData>();
        foreach (var item in Items)
        {
            if (item.Type == type) list.Add(item);
        }
        return list;
    }

    public void SetCurrency(Dictionary<CurrencyType, int> currencies)
    {
        Currencies = currencies;
    }
}
