using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum ShopSellType
{
    SingleItem,
    Bundle
}

public enum ShopLimitType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Lifetime
}

public class ShopProductConfig 
{
    [JsonProperty("product_id")]
    public string ProductID;

    [JsonProperty("shop_category")]
    public string ShopCategory;

    [JsonProperty("sub_category")]
    public string SubCategory;

    [JsonProperty("sell_type")]
    public ShopSellType SellType;

    [JsonProperty("reference_id")]
    public string ReferenceID;

    [JsonProperty("item_amount")]
    public int ItemAmount;

    [JsonProperty("currency_type")]
    public string CurrencyType;

    [JsonProperty("price")]
    public float Price;

    [JsonProperty("original_price")]
    public float OriginalPrice;

    [JsonProperty("limit_count")]
    public int LimitCount;

    [JsonProperty("limit_type")]
    public string RawLimitType;

    [JsonIgnore]
    public ShopLimitType LimitType
    {
        get
        {
            if (string.IsNullOrEmpty(RawLimitType)) return ShopLimitType.None;
            if (Enum.TryParse<ShopLimitType>(RawLimitType, true, out var result)) return result;
            return ShopLimitType.None;
        }
    }

    [JsonProperty("start_time")]
    public string StartTime;

    [JsonProperty("end_time")]
    public string EndTime;

    [JsonProperty("is_active")]
    public bool IsActive;

    [JsonProperty("sort_order")]
    public int SortOrder;

    [JsonProperty("bundle_contents")]
    public List<BundleContentConfig> BundleContents;
}

[Serializable]
public class BundleContentConfig
{
    [JsonProperty("item_id")]
    public string ItemID;

    [JsonProperty("amount")]
    public int Amount;
}
