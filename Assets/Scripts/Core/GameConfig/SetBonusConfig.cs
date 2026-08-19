using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class SetBonusEntry
{
    [JsonProperty("stat")]
    public StatType Stat;

    [JsonProperty("value")]
    public float Value;

    [JsonProperty("modifier_type")]
    public ModifyType Modifier;
}

[Serializable]
public class SetBonusConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("pieces")]
    public int Pieces;

    [JsonProperty("stat")]
    public StatType Stat;

    [JsonProperty("value")]
    public float Value;

    [JsonProperty("modifier_type")]
    public ModifyType Modifier;

    [JsonProperty("stats")]
    public List<SetBonusEntry> Stats;

    public string GetTitleSetBonus()
    {
        return string.Format(LocalizationManager.Instance.GetLocalizedValue("STR_SET_BONUS"), Pieces);
    }

    public string GetConentBonus()
    {
        if (Stats != null && Stats.Count > 0)
        {
            var parts = new List<string>();
            foreach (var s in Stats)
            {
                parts.Add(string.Format(LocalizationManager.Instance.GetLocalizedValue("UI_SET_BONUS_CONTENT"),
                    Utility.GetContextByStatType(s.Stat), Utility.GetConvertStatValueToString(s.Value, s.Modifier)));
            }
            return string.Join(" ", parts);
        }

        return string.Format(LocalizationManager.Instance.GetLocalizedValue("UI_SET_BONUS_CONTENT"),
            Utility.GetContextByStatType(Stat), Utility.GetConvertStatValueToString(Value, Modifier));
    }
}
