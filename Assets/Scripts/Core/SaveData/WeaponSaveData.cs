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
    public int CurrentLevel = 1;

    [JsonProperty("current_updgrade")]
    public int CurrentUpgrade = 1;

    [JsonProperty("equip")]
    public string Equip;
}
