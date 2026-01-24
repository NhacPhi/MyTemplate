using System;

public struct DamageBonus 
{
    public float FlatValue;
    public float DamageMultiplier;

    public static DamageBonus GetDefault()
    {
        return new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1f
        };
    }

    public static DamageBonus operator +(DamageBonus a, DamageBonus b)
    {
        return new DamageBonus()
        {
            FlatValue = a.FlatValue + b.FlatValue,
            DamageMultiplier = a.DamageMultiplier + (1 - b.DamageMultiplier)
        };
    }
}
