using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class StarUpConfig
{
    [JsonProperty("max_tier")]
    public int MaxTier;

    [JsonProperty("tiers")]
    public Dictionary<string, StarUpTierConfig> Tiers;
}

[Serializable]
public class StarUpTierConfig
{
    [JsonProperty("cost_coin")]
    public int CostCoin;

    [JsonProperty("quantity")]
    public int Quantity;
}
