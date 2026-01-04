using Newtonsoft.Json;
using System.Collections.Generic;
public class PartSaveData
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("type")]
    public ArmorPart Type;
}
