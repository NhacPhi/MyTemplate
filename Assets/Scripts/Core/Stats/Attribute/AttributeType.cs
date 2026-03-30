using System;


public enum AttributeType
{
    // Resouce Pool
    Hp,
    Shield,
    Energy,
    // Substats
    ActionPoin, //Peeds
    Fury,
    // Buff/Debuff or Duration-based
    BuffStacks,
    BreakValue //Max Toughness
}

public class AttributeTypeExtentions
{
    public const string HP = "Hp";
    public const string SHIELD = "Shield";


    public static string GetName(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.Hp:
                return HP;
            case AttributeType.Shield:
                return SHIELD;
        }

        return string.Empty;
    }
}
