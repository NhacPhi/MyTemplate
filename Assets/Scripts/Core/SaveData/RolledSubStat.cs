using System;
using Newtonsoft.Json;

[Serializable]
public class RolledSubStat
{
    [JsonProperty("type")]
    public StatType Type { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("modifier_type")]
    public ModifyType ModifierType { get; set; }

    [JsonIgnore]
    private int _calculatedValue;

    /// <summary>
    /// Thuộc tính Value tự động tính toán động dựa vào Level (số lần roll) và Config.
    /// </summary>
    [JsonIgnore]
    public int Value
    {
        get
        {
            if (Level <= 0) Level = 1;
            return _calculatedValue;
        }
    }

    public RolledSubStat()
    {
        Level = 1;
    }

    // Constructor khi đập mới
    public RolledSubStat(StatType type, ModifyType modType, int level = 1)
    {
        this.Type = type;
        this.Level = level <= 0 ? 1 : level;
        this.ModifierType = modType;
    }

    // Constructor cũ hỗ trợ tương thích tạm thời
    public RolledSubStat(StatType type, int initialValue, ModifyType modType)
    {
        this.Type = type;
        this.Level = 1;
        this._calculatedValue = initialValue;
        this.ModifierType = modType;
    }

    public void SetCalculatedValue(int calculatedVal)
    {
        this._calculatedValue = calculatedVal;
    }

    // Hàm để tăng chỉ số khi Upgrade (chỉ tăng Level/số lần roll)
    public void Upgrade(int bonusRolls = 1)
    {
        this.Level += bonusRolls;
    }
}

