using System;
using Newtonsoft.Json;

[Serializable]
public class RolledSubStat
{
    [JsonProperty("type")]
    public StatType Type { get; private set; }

    [JsonProperty("value")]
    public int Value { get; private set; }

    [JsonProperty("level")]
    public int Level { get; private set; }

    [JsonProperty("modifier_type")]
    public ModifyType ModifierType { get; private set; }
}
