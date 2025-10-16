using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


[Serializable]
public class PlayerSave
{
    private string playerName;
    private int level;
    private int currentExp;
    private string avatarIcon;

    public Dictionary<CurrencyType, int> currencies;

    public string PlayerName { get { return playerName; } set { playerName = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int CurrentExp { get { return currentExp; } set { currentExp = value; } }
    public string AvatarIcon { get { return avatarIcon; } set { avatarIcon = value; } }
    public Dictionary<CurrencyType, int> Currencies { get { return currencies; } set { currencies = value; } }
 
    public PlayerSave() { }
}
