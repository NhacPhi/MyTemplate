using System;
using System.Collections.Generic;

public struct DamageBonus 
{
    public float FlatValue;
    public float DamageMultiplier;
    public HashSet<string> Tags;

    public static DamageBonus GetDefault()
    {
        return new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1f,
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
            Tags = newTags
        };
    }
}
