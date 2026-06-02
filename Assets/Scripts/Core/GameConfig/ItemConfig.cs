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

    // Food
    [JsonProperty("food_data")]
    public FoodComponent Food;

    public string GetFormattedUseDescription()
    {
        string rawUseDes = LocalizationManager.Instance.GetLocalizedValue(UseDescription);
        if (string.IsNullOrEmpty(rawUseDes)) return string.Empty;

        if (Type == ItemType.Exp && Exp != null)
        {
            return string.Format(rawUseDes, Exp.Value);
        }
        else if (Type == ItemType.Food && Food != null && Food.Effects != null && Food.Effects.Count > 0)
        {
            var effect = Food.Effects[0];
            try
            {
                return string.Format(rawUseDes, effect.Value.ToString("F0"));
            }
            catch (System.FormatException)
            {
                return rawUseDes;
            }
        }
        
        return rawUseDes;
    }

    public string GetFormattedDescription()
    {
        string desc = LocalizationManager.Instance.GetLocalizedValue(Description);
        if (string.IsNullOrEmpty(desc)) 
            return Description.ToString();
            
        if (Type == ItemType.Shard)
        {
            try
            {
                desc = string.Format(desc, LocalizationManager.Instance.GetLocalizedValue(Name), 60);
            }
            catch (FormatException) { }
        }
        
        return desc;
    }

    public string GetFullFormattedDescription(GameDataBase db = null)
    {
        string desc = GetFormattedDescription();
        string useDesc = GetFormattedUseDescription();
        
        string statDesc = "";
        
        if (!string.IsNullOrEmpty(useDesc))
        {
            statDesc += useDesc;
        }
        
        if (db != null)
        {
            if (Type == ItemType.Weapon && Weapon != null && !string.IsNullOrEmpty(Weapon.PassiveID))
            {
                var passiveConfig = db.GetPassiveConfig(Weapon.PassiveID);
                if (passiveConfig != null)
                {
                    string skillDesc = passiveConfig.GetDescription(1);
                    if (!string.IsNullOrEmpty(skillDesc))
                    {
                        if (!string.IsNullOrEmpty(statDesc)) statDesc += "\n\n";
                        statDesc += skillDesc;
                    }
                }
            }
            else if (Type == ItemType.Armor && Armor != null && !string.IsNullOrEmpty(Armor.ArmorSet))
            {
                var setBonus = db.GetSetBonusConfig(Armor.ArmorSet);
                if (setBonus != null)
                {
                    string setTitle = setBonus.GetTitleSetBonus();
                    string setContent = setBonus.GetConentBonus();
                    if (!string.IsNullOrEmpty(statDesc)) statDesc += "\n\n";
                    statDesc += "<color=#FFD700>" + setTitle + "</color>\n" + setContent;
                }
            }
        }
        
        if (!string.IsNullOrEmpty(statDesc))
        {
            return statDesc.Trim();
        }
        
        return desc;
    }
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

public enum FoodEffectType
{
    RestoreEnergy,
    GlobalStatBuff
}

[Serializable]
public class FoodEffectData
{
    [JsonProperty("effect_type")] public FoodEffectType EffectType;
    [JsonProperty("value")] public float Value;
    [JsonProperty("duration_minutes")] public float DurationMinutes;
    [JsonProperty("stat_type")] public StatType StatType; 
    [JsonProperty("mod_type")] public ModifyType ModifierType;
}

[Serializable]
public class FoodComponent
{
    [JsonProperty("effects")]
    public List<FoodEffectData> Effects;
}

