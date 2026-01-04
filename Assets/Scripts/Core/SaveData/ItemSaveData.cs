using Newtonsoft.Json;
using System;

[Serializable]
public class ItemSaveData 
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("type")]
    public ItemType Type;

    [JsonProperty("quantity")]
    public int Quantity;

}
