using System;
using System.Collections.Generic;

public struct DamageBonus 
{
    public float FlatValue;
    public float DamageMultiplier;
    public float PenetrationBonus;
    public float CritDmgBonus;
    public float CritRateBonus;
    public HashSet<string> Tags;

    public static DamageBonus GetDefault()
    {
        return new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1f,
            PenetrationBonus = 0f,
            CritDmgBonus = 0f,
            CritRateBonus = 0f,
            Tags = new HashSet<string>()
        };
    }

    public static DamageBonus operator +(DamageBonus a, DamageBonus b)
    {
        var newTags = new HashSet<string>();
        if (a.Tags != null) { foreach(var t in a.Tags) newTags.Add(t); }
        if (b.Tags != null) { foreach(var t in b.Tags) newTags.Add(t); }

        return new DamageBonus()
        {
            FlatValue = a.FlatValue + b.FlatValue,
            DamageMultiplier = a.DamageMultiplier + (1 - b.DamageMultiplier),
            PenetrationBonus = a.PenetrationBonus + b.PenetrationBonus,
            CritDmgBonus = a.CritDmgBonus + b.CritDmgBonus,
            CritRateBonus = a.CritRateBonus + b.CritRateBonus,
            Tags = newTags
        };
    }

    public DamageBonus AddTag(string tag)
    {
        if (Tags == null) Tags = new HashSet<string>();
        if (!string.IsNullOrEmpty(tag)) Tags.Add(tag);
        return this;
    }

    public bool HasTag(string tag)
    {
        return Tags != null && Tags.Contains(tag);
    }
}
