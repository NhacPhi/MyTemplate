using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ArmorSaveData
{
    [JsonProperty("instance_id")]
    public string InstanceID;

    [JsonProperty("template_id")]
    public string TemplateID;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("rare")]
    public Rare Rare;

    [JsonProperty("stats")]
    public List<ArmorStatSaveData> Stats;

    [JsonProperty("equip")]
    public string Equip;
}
