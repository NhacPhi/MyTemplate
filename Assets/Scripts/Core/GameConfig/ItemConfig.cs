using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

// Create Item Config by Composition design
public enum ItemType
{
    None,
    Weapon,
    Food,
    Currency,
    Gemstone,
    Exp,
    Shard,
    Armor,
    Item, /// ???
    Avatar,
    Material
}

public enum WeaponType
{
    None,
    Fighter,
    Assassin,
    Mage,
    Tanker,
    Normal,
    Support
}


public class ItemConfig 
{
    // Base 
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("description_hash")]
    public long Description;

    [JsonProperty("use_hash")]
    public long UseDescription;

    [JsonProperty("rarity")]
    public Rare Rarity;

    [JsonProperty("item_type")]
    public ItemType Type;

    [JsonIgnore]
    public Sprite Icon { get; set; }

    [JsonIgnore]
    public Sprite IconBG { get; set; }

    [JsonIgnore]
    public string AtlasAddress
    {
        get
        {
            return Type switch
            {
                ItemType.Weapon => "Atlas_icon_weapon",
                ItemType.Armor => "Atlas_icon_armor",
                ItemType.Gemstone => "Atlas_icon_gemstone",
                ItemType.Shard=> "Atlas_icon_character",
                ItemType.Food or ItemType.Currency or ItemType.Exp => "Atlas_consumables",
                ItemType.Avatar => "Atlas_icon_game",
                _ => "Atlas_icon_game"//default
            };
        }
    }

    // Compositon

    // Weapon
    [JsonProperty("weapon_data")]
    public WeaponComponent Weapon;

    // Exp
    [JsonProperty("exp_data")]
    public ExpComponent Exp;

    // Armor
    [JsonProperty("armor_data")]
    public ArmorComponent Armor;
}

[Serializable]
public class WeaponComponent
{
    [JsonProperty("weapon_type")]
    public WeaponType Type;

    [JsonProperty("passive_id")]
    public string PassiveID;

    [JsonProperty("stats")]
    public Dictionary<StatType, int> Stats;

    [JsonProperty("upgrades")]
    public Dictionary<StatType, int> Upgrades;

    [JsonIgnore]
    public Sprite BigIcon { get; set; }

    public int GetStatByLevel(StatType type, int level)
    {
        return Stats[type] + Utility.GetStatGrowthLevel(level, Upgrades[type]);
    }
}

[Serializable]
public class ExpComponent
{
    [JsonProperty("value")]
    public int Value;
}

[Serializable]
public class ArmorComponent
{
    [JsonProperty("part")]
    public ArmorPart Part;

    [JsonProperty("armor_set")]
    public string ArmorSet;

    [JsonProperty("main_stat")]
    public MainStatConfig MainStat;

    [JsonProperty("substat_pool_id")]
    public string SubstatPoolID; //List<SubstatCompoment>
}

[Serializable]
public class SubstatCompoment
{
    [JsonProperty("stat_type")]
    public StatType Type;

    [JsonProperty("min")]
    public float Min;

    [JsonProperty("max")]
    public float Max;

    [JsonProperty("modifier_type")] 
    public ModifyType ModifierType;
}

[Serializable]
public class MainStatConfig
{
    [JsonProperty("type")] public StatType Type;
    [JsonProperty("value")] public float Value;
    [JsonProperty("mod_type")] public ModifyType ModifierType;
}


