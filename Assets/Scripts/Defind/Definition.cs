using System.Collections.Generic;
using UnityEngine;

public static class Definition
{
    public readonly static List<string> SettingsLanguage = new List<string> { "ENGLISH", "VIETNAMESE" };
    public readonly static List<string> SettingsFPS = new List<string> { "30", "60" };

    public readonly static Color32 SeletedColor = new Color32(248, 233, 13, 255);

    public readonly static Color32 LegendaryColor = new Color32(245, 201, 13, 255);
    public readonly static Color32 EpicColor = new Color32(245, 81, 31, 255);
    public readonly static Color32 RareColor = new Color32(220, 31, 245, 255);
    public readonly static Color32 UncommonColor = new Color32(31, 194, 245, 255);
    public readonly static Color32 CommonColor = new Color32(162, 209, 233, 255);
}
 
public enum Rare
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemType
{
    Item,
    Armor,
    Weapon,
    Shard,
    Material,
    Avatar,
    Food,
    GemStone
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


