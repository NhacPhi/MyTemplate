using System.Collections.Generic;
using UnityEngine;

public static class Definition
{
    public readonly static List<string> SettingsLanguage = new List<string> { "ENGLISH", "VIETNAMESE" };
    public readonly static List<string> SettingsFPS = new List<string> { "30", "60" };

    public readonly static Color32 SeletedColor = new Color32(248, 233, 13, 255);
    public readonly static Color32 OriginColor = new Color32(255, 255, 255, 255);
    public readonly static Vector3 scale = new Vector3(1.1F, 1.1F, 1.1F);

    public readonly static Color32 LegendaryColor = new Color32(245, 201, 13, 255);
    public readonly static Color32 EpicColor = new Color32(245, 81, 31, 255);
    public readonly static Color32 RareColor = new Color32(220, 31, 245, 255);
    public readonly static Color32 UncommonColor = new Color32(31, 194, 245, 255);
    public readonly static Color32 CommonColor = new Color32(162, 209, 233, 255);

    public static int CharacterMaxLevel = 10;
}
 
public enum Rare
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
public enum CharacterRare
{
    R,
    SR,
    SSR,
    UR
}

public enum CharacterType
{
    Fighter,
    ADCarry,
    Assassin,
    Mage,
    Support,
    Tanker,
    Normal
}


public enum ItemType
{
    None,
    Item,
    Armor,
    Weapon,
    Shard,
    Material,
    Avatar,
    Food,
    GemStone,
    Exp
}

public enum WeaponType
{
    Fighter,
    Assassin,
    Tank,
    Mage,
    Support,
    Normal
}

public enum ArmorPart
{
    Helmet,
    Chestplate,
    Gloves,
    Boots,
    Belt,
    Ring
}

public enum StatPool
{
    HP,
    ATK,
    DEF,
    SPD,
    ARM_PEN,
    CRIT,
    CIRT_DMG,
    EFF_RES,
    ACC
}
// Stat Growth level
//Stat(level)=Base+Growth×(level?1)×(0.7+0.03×(level?1))

//Exp to upgrade
//ExpRequired(n)=1800+1000×(n?1)+600×(n?1)2

