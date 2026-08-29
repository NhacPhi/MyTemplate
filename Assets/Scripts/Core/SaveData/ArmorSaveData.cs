using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ArmorSaveData
{
    [JsonProperty("instance_id")]
    public string UUID;

    [JsonProperty("template_id")]
    public string TemplateID;

    [JsonProperty("level")]
    public int Level;

    [JsonIgnore]
    private Rare? _rare = null;

    [JsonIgnore]
    public Rare Rare
    {
        get
        {
            if (!_rare.HasValue && GameDataBase.Instance != null && !string.IsNullOrEmpty(TemplateID))
            {
                var config = GameDataBase.Instance.GetItemConfig(TemplateID);
                if (config != null)
                {
                    _rare = config.Rarity;
                }
            }
            return _rare.GetValueOrDefault(Rare.Common);
        }
        set
        {
            _rare = value;
        }
    }

    [JsonProperty("substats")]
    public List<RolledSubStat> Substats;

    [JsonProperty("main_stat_type")]
    public StatType MainStatType = StatType.None;

    [JsonProperty("equip")]
    public string Equip;
}
