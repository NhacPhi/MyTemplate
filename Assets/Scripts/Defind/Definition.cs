using System.Collections.Generic;


public static class Definition
{
    public readonly static List<string> SettingsLanguage = new List<string> { "ENGLISH", "VIETNAMESE" };
    public readonly static List<string> SettingsFPS = new List<string> { "30", "60" };
}

public enum Rare
{
    Common,
    Uncommon,
    Epic,
    Legendary
}