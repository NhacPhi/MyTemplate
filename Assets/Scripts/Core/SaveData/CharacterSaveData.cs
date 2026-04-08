using Newtonsoft.Json;
using System.Collections.Generic;
using System;

[Serializable]
public class CharacterSaveData
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("level")]
    public int Level;

    [JsonProperty("exp")]
    public int Exp;

    [JsonProperty("ascension_tier")]
    public int AscensionTier;

    [JsonProperty("star_up")]
    public int StarUp; //Get shard to upgrade

    [JsonProperty("weapon")]
    public string Weapon;

    [JsonProperty("armors")]
    public List<PartSaveData> Armors;
}
