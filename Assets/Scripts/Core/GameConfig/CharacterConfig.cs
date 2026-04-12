using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Tech.Logger;

[Serializable]
public class CharacterConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("rare")]
    public CharacterRare Rare;

    [JsonProperty("type")]
    public CharacterType Type;

    [JsonProperty("stats")]
    public Dictionary<StatType, int> Stats;

    [JsonProperty("attributes")]
    public Dictionary<AttributeType, AttributeComponent> Attributes;

    [JsonProperty("upgrades")]
    public Dictionary<StatType, int> Upgrades;

    [JsonProperty("skills")]
    public Dictionary<SkillCharacter, SkillComponent> Skills;

    [JsonIgnore]
    public Sprite Icon { get; set; }

    [JsonIgnore]
    public Sprite BigIcon { get; set; }

    [JsonIgnore]
    public Sprite Image { get; set; }

    [JsonIgnore]
    public Sprite BaseSkillIcon { get; set; }

    [JsonIgnore]
    public Sprite MajorSkillIcon { get; set; }

    [JsonIgnore]
    public Sprite UltimateSkillIcon { get; set; }
    public AttributeComponent GetAttribute(AttributeType type)
    {
        if (Attributes.TryGetValue(type, out AttributeComponent value))
        {
            return value;
        }

        LogCommon.LogWarning($"{type} not Found");
        return default;
    }

    public int GetStat(StatType type)
    {
        if (Stats.TryGetValue(type, out int value))
        {
            return value;
        }

        LogCommon.LogWarning($"{type} not Found");
        return default;
    }

    public int GetUpdateStat(StatType type)
    {
        if (Upgrades.TryGetValue(type, out int value))
        {
            return value;
        }

        LogCommon.LogWarning($"{type} not Found");
        return default;
    }

    public int GetStatByLevel(StatType type, int level)
    {
        return Stats[type] + Utility.GetStatGrowthLevel(level, GetUpdateStat(type));
    }
}
[Serializable]
public class AttributeComponent
{
    [JsonProperty("max_stat_type")]
    public StatType StatMaxStatType;

    [JsonProperty("start_percent")]
    public float StartPercent;
}

[Serializable]
public class SkillComponent
{
    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("name_hash")]
    public long Name { get; set; }

    [JsonProperty("des_hash")]
    public long Description { get; set; }

    [JsonProperty("skill")]
    public Skill Skill { get; set; }

    [JsonProperty("skill_type")]
    public SkillType Type { get; set; }

    [JsonProperty("target_type")]
    public SkillTargetType TargetType { get; set;}

    [JsonProperty("damage_multiplier")]
    public float DamageMultiplier { get; set; }

    [JsonProperty("max_cooldown")]
    public int MaxCooldown { get; set; }

    [JsonProperty("flat_damage")]
    public float FlatDamage { get; set; }

    [JsonProperty("effect_id")]
    public string EffectID { get; set; }

    [JsonProperty("sound")]
    public string Sound { get; set; }
}
