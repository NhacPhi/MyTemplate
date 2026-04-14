using System;
using Newtonsoft.Json;

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

    public string GetTitleSetBonus()
    {
        return string.Format(LocalizationManager.Instance.GetLocalizedValue("STR_SET_BONUS"), Pieces);
    }

    public string GetConentBonus()
    {
        return string.Format(LocalizationManager.Instance.GetLocalizedValue("UI_SET_BONUS_CONTENT"),
            Utility.GetContextByStatType(Stat), Utility.GetConvertStatValueToString(Value, Modifier));
    }
}
