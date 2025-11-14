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
    private List<WeaponData> weapons;
    List<ItemData> items;
    List<ArmorData> armors;
    List<CharacterData> characters;

    public string PlayerName { get { return playerName; } set { playerName = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int CurrentExp { get { return currentExp; } set { currentExp = value; } }
    public string AvatarIcon { get { return avatarIcon; } set { avatarIcon = value; } }
    public Dictionary<CurrencyType, int> Currencies { get { return currencies; } set { currencies = value; } }
    public List<WeaponData> Weapons { get { return weapons; } set { weapons = value; } }
    public List<ItemData> Items { get { return items; } set { items = value; } }
    public List<ArmorData> Armors { get { return armors; } set { armors = value; } }
    public List<CharacterData> Characters { get { return characters; } set { characters = value; } }
    public PlayerSave() { }

    public WeaponData GetWeapon(string id)
    {
        return Weapons.Find(v => v.ID == id);
    }

    public ItemData GetItem(string id)
    {
        return items.Find(v => v.ID == id);
    }

    public ArmorData GetArmor(string id)
    {
        return armors.Find(v => v.InstanceID == id);
    }

    public CharacterData GetCharacter(string id)
    {
        return characters.Find(v => v.ID == id);
    }

    public CharacterData GetIDOfFirstCharacter()
    {
        return characters[0];
    }
    public List<ItemData> GetAllItemByType(ItemType type)
    {
        List<ItemData> list = new List<ItemData>();
        foreach(var item in items)
        {
            if(item.Type == type) list.Add(item);
        }
        return list;
    }
}
