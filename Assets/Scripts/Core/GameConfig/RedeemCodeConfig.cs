using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class RedeemRewardData
{
    [JsonProperty("type")]
    public string Type;

    [JsonProperty("id")]
    public string Id;

    [JsonProperty("amount")]
    public int Amount;
}

[Serializable]
public class RedeemCodeConfig
{
    [JsonProperty("code")]
    public string Code;

    [JsonProperty("is_active")]
    public bool IsActive;

    [JsonProperty("rewards")]
    public List<RedeemRewardData> Rewards = new List<RedeemRewardData>();
}
