using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class BannerRuntimeData 
{
    [JsonProperty("banner_id")]
    public string BannerId;
    
    [JsonProperty("pity_count")]
    public int PityCount; // Đếm số lần chưa ra 5 sao
    
    [JsonProperty("selected_target_id")]
    public string SelectedTargetId; // Mục tiêu người chơi đang chọn
    
    [JsonProperty("is_next_ssr_guaranteed")]
    public bool IsNextSSRGuaranteed; // Trạng thái bảo hiểm 50/50
    
    [JsonProperty("total_summon_count")]
    public int TotalSummonCount; // Tổng số lần đã quay
}

[Serializable]
public class GachaSaveData
{
    [JsonProperty("banner_data_map")]
    public Dictionary<string, BannerRuntimeData> BannerDataMap { get; private set; } = new Dictionary<string, BannerRuntimeData>();
}
