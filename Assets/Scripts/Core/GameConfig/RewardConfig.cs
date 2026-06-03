using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class RewardConfig
{
    [JsonProperty("reward_id")]
    public string RewardID;

    [JsonProperty("rewards")]
    public List<RewardItemConfig> Rewards;
}

[Serializable]
public class RewardItemConfig
{
    [JsonProperty("item_id")]
    public string ItemID;

    [JsonProperty("amount")]
    public int Amount;
}
