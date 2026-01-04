using System;
using Newtonsoft.Json;

[Serializable]
public class ArmorStatSaveData
{
    [JsonProperty("type")]
    public StatType Type;

    [JsonProperty("point")]
    public int Point;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("modifier_type")]
    public ModifyType ModifierType;
}
