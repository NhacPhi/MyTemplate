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

    //
    public static int MAX_ARMOR_LEVEL = 15;
    public static int MAX_ARMOR_SUBSTATS = 4;
    public static int ARMOR_SUBSTAT_INTERVAL = 3;
    public static int MAX_WEAPON_LEVEL = 100;
    public static int MAX_WEAPON_ASCEND = 6;

    public static int MAX_CHARACTER_LEVEL = 100;
    public static int MAX_SLOT_CHARACTER = 6;
}
 
public enum Rare
{
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Epic = 4,
    Legendary =5
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

public enum ArmorPart
{
    Helmet,
    Chestplate,
    Gloves,
    Boots,
    Belt,
    Ring
}




