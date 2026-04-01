using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AscensionConfig 
{
    [JsonProperty("max_tier")]
    public int MaxTier;

    [JsonProperty("tier")]
    public Dictionary<string, TierConfig> TierConfigs;
}

[Serializable]
public class CostIteam
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("quantity")]
    public int Quantity;
}

[Serializable]
public class TierConfig
{
    [JsonProperty("unlock_max_level")]
    public int UnlockMaxLevel;

    [JsonProperty("level_req")]
    public int LevelRequire;

    [JsonProperty("cost")]
    public List<CostIteam> costs;
}
