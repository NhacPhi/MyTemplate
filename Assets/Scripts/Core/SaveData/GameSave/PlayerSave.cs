using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


[Serializable]
public class PlayerSave
{
    [JsonProperty("player_name")]
    public string PlayerName { get; private set; }

    [JsonProperty("level")]
    public int Level { get; private set; }

    [JsonProperty("avatar_icon")]
    public string AvatarIcon { get; private set; }

    [JsonProperty("currencies")]
    public Dictionary<CurrencyType, int> Currencies { get; private set; }

    [JsonProperty("weapons")]
    public List<WeaponSaveData> Weapons { get; private set; }

    [JsonProperty("items")]
    public List<ItemSaveData> Items { get; private set; }

    [JsonProperty("armors")]
    public List<ArmorSaveData> Armors { get; private set; }

    [JsonProperty("characters")]
    public List<CharacterSaveData> Characters;

    public PlayerSave() { }

    public WeaponSaveData GetWeapon(string id)
    {
        return Weapons.Find(v => v.ID == id);
    }

    public ItemSaveData GetItem(string id)
    {
        return Items.Find(v => v.ID == id);
    }

    public ArmorSaveData GetArmor(string id)
    {
        return Armors.Find(v => v.InstanceID == id);
    }

    public CharacterSaveData GetCharacter(string id)
    {
        return Characters.Find(v => v.ID == id);
    }

    public CharacterSaveData GetIDOfFirstCharacter()
    {
        return Characters[0];
    }
    public List<ItemSaveData> GetAllItemByType(ItemType type)
    {
        List<ItemSaveData> list = new List<ItemSaveData>();
        foreach(var item in Items)
        {
            if(item.Type == type) list.Add(item);
        }
        return list;
    }

    public void SetAvatarIcon(string id)
    {
        AvatarIcon = id;
    }

    public void SetCurrency(Dictionary<CurrencyType, int> currencies)
    {
        Currencies = currencies;
    }
}
