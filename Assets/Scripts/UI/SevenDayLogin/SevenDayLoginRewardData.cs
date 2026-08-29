using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UIFramework
{
    public enum SevenDayRewardType
    {
        Item,
        Weapon,
        Armor,
        Character,
        Currency
    }

    public enum SevenDayRewardState
    {
        Locked,     // Chưa đến ngày mở nhận thưởng
        CanClaim,   // Đã mở, sẵn sàng nhận thưởng (Trạng thái 1: có thể nhận)
        Claimed     // Đã nhận thưởng rồi (Trạng thái 2: đã nhận)
    }

    [Serializable]
    public class SevenDayRewardItem
    {
        public int DayNumber;               // Ngày thứ mấy (1 -> 7)
        public SevenDayRewardType Type;     // Loại phần thưởng
        public string RewardId;             // ID vật phẩm / vũ khí / nhân vật / tiền tệ
        public int Amount = 1;              // Số lượng
        public SevenDayRewardState State = SevenDayRewardState.Locked; // Trạng thái
        public string CustomName = "";      // Tên hiển thị tùy chỉnh (nếu có)

        public SevenDayRewardItem() { }

        public SevenDayRewardItem(int dayNumber, SevenDayRewardType type, string rewardId, int amount, SevenDayRewardState state = SevenDayRewardState.Locked, string customName = "")
        {
            DayNumber = dayNumber;
            Type = type;
            RewardId = rewardId;
            Amount = amount;
            State = state;
            CustomName = customName;
        }
    }

    [Serializable]
    public class SevenDayLoginConfigData
    {
        [JsonProperty("day_number")]
        public int DayNumber;

        [JsonProperty("reward_type")]
        public string RewardType;

        [JsonProperty("reward_id")]
        public string RewardId;

        [JsonProperty("amount")]
        public int Amount;

        [JsonProperty("custom_name")]
        public string CustomName;
    }
}
