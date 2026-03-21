using System;
using Newtonsoft.Json;
public enum EffectType
{
    None,

    Poison,
    Stun,
    StatBuff,
    StatDebuff,
    ResetDebuff
}


public class EffectConfig
{
    [JsonProperty("name_hash")]
    public long Name { get; set; }

    [JsonProperty("des_hash")]
    public long Description { get; set; }

    [JsonProperty("type")]
    public EffectType Type { get; set; }

    [JsonProperty("target_stat")]
    public StatType TargetStat { get; set; }

    [JsonProperty("modify_type")]
    public ModifyType ModifyType { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }

    [JsonProperty("max_stack")]
    public int MaxStack { get; set; }

    [JsonProperty("value")]
    public int Value { get; set; }
}
