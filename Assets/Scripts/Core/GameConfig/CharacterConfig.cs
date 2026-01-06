using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


public class CharacterConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("rare")]
    public CharacterRare Rare;

    [JsonProperty("type")]
    public CharacterType Type;

    [JsonProperty("stats")]
    public Dictionary<StatType, int> Stats;

    [JsonProperty("upgrades")]
    public Dictionary<StatType, int> Upgrades;

    [JsonIgnore]
    public Sprite Icon { get; set; }

    [JsonIgnore]
    public Sprite BigIcon { get; set; }
}
