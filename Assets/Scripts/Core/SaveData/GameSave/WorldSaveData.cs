using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum RespawnType
{
    TimeBased,
    DailyReset,
    Never,
    OnSceneReload
}

[Serializable]
public class WorldSaveData
{
    // Dictionary lưu ID của Resource và mốc thời gian (Ticks của UTC Time) mà nó bị tiêu diệt
    [JsonProperty("destroyed_resources")]
    public Dictionary<string, long> DestroyedResources { get; private set; } = new Dictionary<string, long>();

    /// <summary>
    /// Ghi nhận thời điểm chết của vật thể.
    /// </summary>
    public void RecordDestroyed(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        
        DestroyedResources[id] = DateTime.UtcNow.Ticks;
    }

    /// <summary>
    /// Kiểm tra xem vật thể đã thỏa điều kiện hồi sinh chưa.
    /// Nếu đã hồi sinh, tự động xóa nó khỏi danh sách Destroyed.
    /// </summary>
    public bool HasRespawned(string id, RespawnType type, float cooldownMinutes)
    {
        if (string.IsNullOrEmpty(id) || !DestroyedResources.ContainsKey(id))
            return true; // Nếu chưa từng bị phá hủy hoặc ko có ID, coi như còn sống

        long destroyedTicks = DestroyedResources[id];
        DateTime destroyedTime = new DateTime(destroyedTicks, DateTimeKind.Utc);
        DateTime currentTime = DateTime.UtcNow;

        bool respawned = false;

        switch (type)
        {
            case RespawnType.TimeBased:
                // Hồi sinh sau X phút
                if ((currentTime - destroyedTime).TotalMinutes >= cooldownMinutes)
                {
                    respawned = true;
                }
                break;

            case RespawnType.DailyReset:
                // Hồi sinh vào 4:00 AM sáng hôm sau (theo giờ UTC)
                // Hoặc tùy chỉnh theo múi giờ mong muốn. Ở đây dùng UTC.
                DateTime nextReset = destroyedTime.Date.AddHours(4); // 4 AM của ngày bị giết
                if (destroyedTime.Hour >= 4)
                {
                    // Nếu chết sau 4 AM, reset vào 4 AM ngày hôm sau
                    nextReset = nextReset.AddDays(1);
                }

                if (currentTime >= nextReset)
                {
                    respawned = true;
                }
                break;

            case RespawnType.Never:
                // Không bao giờ hồi sinh (Rương, Thần đồng...)
                respawned = false;
                break;
                
            case RespawnType.OnSceneReload:
                // Hồi sinh ngay lập tức mỗi khi load lại Scene
                respawned = true;
                break;
        }

        if (respawned)
        {
            DestroyedResources.Remove(id);
        }

        return respawned;
    }
}
