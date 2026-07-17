using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ShopPurchaseRecord
{
    [JsonProperty("product_id")]
    public string ProductId;

    [JsonProperty("purchase_count")]
    public int PurchaseCount;

    [JsonProperty("last_purchase_time")]
    public long LastPurchaseTimeTicks;
}

[Serializable]
public class ShopSaveData
{
    [JsonProperty("last_reset_time")] 
    public long LastResetTimeTicks { get; set; } = 0;

    // Lịch sử mua hàng
    [JsonProperty("purchase_history")]
    public Dictionary<string, ShopPurchaseRecord> PurchaseHistory { get; private set; }

    // Reset shop purchase history for daily reset
    public void ResetPurchaseHistory()
    {
        PurchaseHistory.Clear();
        LastResetTimeTicks = DateTime.UtcNow.Ticks;
    }

    public ShopSaveData()
    {
        PurchaseHistory = new Dictionary<string, ShopPurchaseRecord>();
    }

    // Hàm tiện ích để lấy dữ liệu mua
    public ShopPurchaseRecord GetRecord(string productId)
    {
        if (PurchaseHistory.TryGetValue(productId, out var record))
        {
            return record;
        }
        return new ShopPurchaseRecord { ProductId = productId, PurchaseCount = 0, LastPurchaseTimeTicks = 0 };
    }

    // Hàm tiện ích để cập nhật số lần mua
    public void AddPurchase(string productId, int amount = 1)
    {
        if (!PurchaseHistory.ContainsKey(productId))
        {
            PurchaseHistory[productId] = new ShopPurchaseRecord
            {
                ProductId = productId,
                PurchaseCount = 0,
                LastPurchaseTimeTicks = DateTime.UtcNow.Ticks
            };
        }
        
        PurchaseHistory[productId].PurchaseCount += amount;
        PurchaseHistory[productId].LastPurchaseTimeTicks = DateTime.UtcNow.Ticks;
    }
}
