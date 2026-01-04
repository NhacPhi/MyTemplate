using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Logger;
using Newtonsoft.Json;

public class StatsDataHolder
{
    [JsonProperty("Stats")]
    public IReadOnlyDictionary<StatType, float> Stats;

    //[JsonProperty("Attribute")]
    //public IReadOnlyDictionary<AttributeType, Attribute> Attributes;

    public float GetStat(StatType type)
    {
        if(Stats.TryGetValue(type, out var value))
        {
            return value;
        }

        LogCommon.LogWarning($"{type} not found");
        return default;
    }

    //public Attribute GetAttribute(AttributeType type)
    //{
    //    if(Attributes.TryGetValue(type, out Attribute value))
    //    {
    //        return value;
    //    }

    //    LogCommon.LogWarning($"{type} not found");
    //    return default;
    //}
}

