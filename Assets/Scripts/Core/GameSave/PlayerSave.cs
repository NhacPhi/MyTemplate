using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


[Serializable]
public class PlayerSave
{
    private string playerName;
    private int level;
    private int currentExp;
    private string avatarIcon;

    private Dictionary<CurrencyType, int> currencies;
    private List<Weapon> weapons;
    List<Item> items;
    

    public string PlayerName { get { return playerName; } set { playerName = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int CurrentExp { get { return currentExp; } set { currentExp = value; } }
    public string AvatarIcon { get { return avatarIcon; } set { avatarIcon = value; } }
    public Dictionary<CurrencyType, int> Currencies { get { return currencies; } set { currencies = value; } }
    public List<Weapon> Weapons { get { return weapons; } set { weapons = value; } }
    public List<Item> Items { get { return items; } set { items = value; } }
    public PlayerSave() { }

    public Weapon GetWeapon(string id)
    {
        return Weapons.Find(v => v.ID == id);
    }

    public Item GetItem(string id)
    {
        return items.Find(v => v.ID == id);
    }

    public List<Item> GetAllItemByType(ItemType type)
    {
        List<Item> list = new List<Item>();
        foreach(var item in items)
        {
            if(item.Type == type) list.Add(item);
        }
        return list;
    }
}
