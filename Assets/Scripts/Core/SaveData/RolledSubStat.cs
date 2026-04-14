using System;
using Newtonsoft.Json;

[Serializable]
public class RolledSubStat
{
    [JsonProperty("type")]
    public StatType Type { get; private set; }

    [JsonProperty("value")]
    public int Value { get; private set; }

    [JsonProperty("level")]
    public int Level { get; private set; }

    [JsonProperty("modifier_type")]
    public ModifyType ModifierType { get; private set; }

    // Thêm Constructor để khởi tạo từ Pool
    public RolledSubStat(StatType type, int value, ModifyType modType)
    {
        this.Type = type;
        this.Value = value;
        this.Level = 1;
        this.ModifierType = modType;
    }

    // Hàm để tăng chỉ số khi Upgrade
    public void Upgrade(int bonusValue)
    {
        this.Value += bonusValue;
        this.Level++;
    }
}
