using Newtonsoft.Json;
using System;

[System.Serializable]
public class ActiveSlotData
{
    [JsonProperty("position")]
    public int Position;

    [JsonProperty("character_id")]
    public string CharacterID;
}
