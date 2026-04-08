using System;
using Newtonsoft.Json;

[Serializable]
public class SetBonusConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("pieces")]
    public int Pieces;

    [JsonProperty("stat")]
    public StatType Stat;

    [JsonProperty("value")]
    public float Value;

    [JsonProperty("modifier_type")]
    public ModifyType Modifier;
}
