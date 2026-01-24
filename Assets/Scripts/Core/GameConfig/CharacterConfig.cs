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

    [JsonIgnore]
    public Sprite Icon { get; set; }

    [JsonIgnore]
    public Sprite BigIcon { get; set; }

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
}
[Serializable]
public class AttributeComponent
{
    [JsonProperty("max_stat_type")]
    public StatType StatMaxStatType;

    [JsonProperty("start_percent")]
    public float SttartPercent;
}
