using Newtonsoft.Json;
using System;

[System.Serializable]
public class WeaponSaveData
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("current_level")]
    public int CurrentLevel;

    [JsonProperty("current_updgrade")]
    public int CurrentUpgrade;

    [JsonProperty("equip")]
    public string Equip;
}
