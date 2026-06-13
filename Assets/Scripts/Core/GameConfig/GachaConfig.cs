using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum GachaBannerType
{
    Character,
    Weapon
}

[Serializable]
public class GachaCostConfig
{
    [JsonProperty("type")]
    public string Type;

    [JsonProperty("amount")]
    public int Amount;
}

[Serializable]
public class GachaRateConfig
{
    [JsonProperty("baseRate")]
    public float BaseRate;

    [JsonProperty("isGuarantee")]
    public bool IsGuarantee;
}

[Serializable]
public class GachaPoolItem
{
    [JsonProperty("itemId")]
    public string ItemId;

    [JsonProperty("rarity")]
    public int Rarity;

    [JsonProperty("isRateUp")]
    public bool IsRateUp;

    [JsonProperty("isSelectableTarget")]
    public bool IsSelectableTarget;

    [JsonProperty("weight")]
    public int Weight;
}

public class GachaConfig
{
    [JsonProperty("bannerId")]
    public string BannerId;

    [JsonProperty("name_hash")]
    public long NameHash;

    [JsonProperty("type")]
    public GachaBannerType Type;

    [JsonProperty("pityLimit")]
    public int PityLimit;

    [JsonProperty("cost")]
    public GachaCostConfig Cost;

    [JsonProperty("allowSelection")]
    public bool AllowSelection;

    [JsonProperty("rates")]
    public Dictionary<string, GachaRateConfig> Rates;

    [JsonProperty("pool")]
    public List<GachaPoolItem> Pool;
}
