using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ArmorSaveData
{
    [JsonProperty("instance_id")]
    public string UUID;

    [JsonProperty("template_id")]
    public string TemplateID;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("rare")]
    public Rare Rare;

    [JsonProperty("substats")]
    public List<RolledSubStat> Substats;

    [JsonProperty("equip")]
    public string Equip;
}
