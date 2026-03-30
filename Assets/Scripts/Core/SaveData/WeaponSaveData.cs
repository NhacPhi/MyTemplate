using Newtonsoft.Json;
using System;

[System.Serializable]
public class WeaponSaveData
{
    [JsonProperty("uuid")]
    public string UUID;

    [JsonProperty("template_id")]
    public string TemplateID;

    [JsonProperty("current_level")]
    public int CurrentLevel;

    [JsonProperty("current_updgrade")]
    public int CurrentUpgrade;

    [JsonProperty("equip")]
    public string Equip;
}
