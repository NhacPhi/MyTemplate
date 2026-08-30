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

    public ShopSaveData()
    {
        PurchaseHistory = new Dictionary<string, ShopPurchaseRecord>();
    }

    // Reset toàn bộ lịch sử mua hàng
    public void ResetPurchaseHistory()
    {
        PurchaseHistory.Clear();
        LastResetTimeTicks = DateTime.UtcNow.Ticks;
    }

    /// <summary>
    /// Kiểm tra và tự động xóa các lượt mua đã hết hạn (Daily qua ngày mới, Weekly qua tuần mới)
    /// </summary>
    public void CheckAndResetShopLimits(GameDataBase gameDataBase)
    {
        if (gameDataBase == null || PurchaseHistory == null || PurchaseHistory.Count == 0) return;

        DateTime nowUtc = DateTime.UtcNow;
        List<string> expiredKeys = new List<string>();

        foreach (var kvp in PurchaseHistory)
        {
            var productId = kvp.Key;
            var record = kvp.Value;
            var config = gameDataBase.GetShopProductConfig(productId);

            if (config == null) continue;

            if (IsRecordExpired(record, config.LimitType, nowUtc))
            {
                expiredKeys.Add(productId);
            }
        }

        foreach (var key in expiredKeys)
        {
            PurchaseHistory.Remove(key);
        }

        LastResetTimeTicks = nowUtc.Ticks;
    }

    /// <summary>
    /// Kiểm tra xem 1 record mua hàng đã hết hạn theo LimitType hay chưa
    /// </summary>
    public bool IsRecordExpired(ShopPurchaseRecord record, ShopLimitType limitType, DateTime nowUtc)
    {
        if (record == null || record.LastPurchaseTimeTicks <= 0) return false;
        if (limitType == ShopLimitType.None || limitType == ShopLimitType.Lifetime) return false;

        DateTime lastPurchaseUtc = new DateTime(record.LastPurchaseTimeTicks, DateTimeKind.Utc);

        if (limitType == ShopLimitType.Daily)
        {
            // Mốc reset ngày: 04:00 AM UTC
            DateTime todayReset = nowUtc.Hour >= 4 ? nowUtc.Date.AddHours(4) : nowUtc.Date.AddDays(-1).AddHours(4);
            return lastPurchaseUtc < todayReset;
        }

        if (limitType == ShopLimitType.Weekly)
        {
            // Mốc reset tuần: 04:00 AM sáng Thứ Hai
            int daysSinceMonday = ((int)nowUtc.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime thisMonday = nowUtc.Date.AddDays(-daysSinceMonday);
            DateTime thisWeekReset = (nowUtc.Hour >= 4 && daysSinceMonday == 0)
                ? thisMonday.AddHours(4)
                : (daysSinceMonday > 0 ? thisMonday.AddHours(4) : thisMonday.AddDays(-7).AddHours(4));

            return lastPurchaseUtc < thisWeekReset;
        }

        if (limitType == ShopLimitType.Monthly)
        {
            DateTime thisMonthReset = new DateTime(nowUtc.Year, nowUtc.Month, 1, 4, 0, 0, DateTimeKind.Utc);
            return lastPurchaseUtc < thisMonthReset;
        }

        return false;
    }

    // Hàm tiện ích để lấy dữ liệu mua (tự động check hạn)
    public ShopPurchaseRecord GetRecord(string productId, GameDataBase gameDataBase = null)
    {
        if (PurchaseHistory.TryGetValue(productId, out var record))
        {
            if (gameDataBase != null)
            {
                var config = gameDataBase.GetShopProductConfig(productId);
                if (config != null && IsRecordExpired(record, config.LimitType, DateTime.UtcNow))
                {
                    PurchaseHistory.Remove(productId);
                    return new ShopPurchaseRecord { ProductId = productId, PurchaseCount = 0, LastPurchaseTimeTicks = 0 };
                }
            }
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
