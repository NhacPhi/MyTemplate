using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

// --- COMPONENT TẦNG 1: CHỈ SỐ TĨNH ---
[Serializable]
public class StaticModifierConfig
{
    [JsonProperty("stat_type")]
    public string StatType;

    [JsonProperty("modify_type")]
    public string ModifyType;

    [JsonProperty("modify_by_upgrade")]
    public List<float> ModifyByUpgrade;
}

// --- COMPONENT TẦNG 2: SỰ KIỆN TRẬN ĐÁNH ---
[Serializable]
public class CombatEventConfig
{
    [JsonProperty("event_type")]
    public string EventType;

    [JsonProperty("effect_id")]
    public string EffectId;

    [JsonProperty("modify_by_upgrade")]
    public List<float> ModifyByUpgrade;

    [JsonProperty("target")]
    public string Target;

    [JsonProperty("condition_filter")]
    public string ConditionFilter;

    [JsonIgnore]
    private HashSet<string> _conditionTags;

    [JsonIgnore]
    public HashSet<string> ConditionTags
    {
        get
        {
            if (_conditionTags == null)
            {
                _conditionTags = new HashSet<string>();
                if (!string.IsNullOrEmpty(ConditionFilter))
                {
                    string[] tags = ConditionFilter.Split(',');
                    foreach (var tag in tags)
                    {
                        string t = tag.Trim();
                        if (!string.IsNullOrEmpty(t))
                        {
                            _conditionTags.Add(t);
                        }
                    }
                }
            }
            return _conditionTags;
        }
    }

    [JsonProperty("effect_param")]
    public float EffectParam;

    [JsonProperty("internal_cooldown")]
    public int InternalCooldown;
}

// --- MASTER CLASS: QUẢN LÝ KỸ NĂNG ---
[Serializable]
public class PassiveConfig
{
    [JsonIgnore]
    public string ID { get; set; }

    [JsonProperty("desc_template_hash")]
    public long DescTemplateHash;

    [JsonProperty("static_modifiers")]
    public List<StaticModifierConfig> StaticModifiers;

    [JsonProperty("combat_events")]
    public List<CombatEventConfig> CombatEvents;

    /// <summary>
    /// Sinh chuỗi mô tả tự động dựa trên cấu trúc Component hiện có
    /// </summary>
    public string GetDescription(int currentUpgrade)
    {
        string template = LocalizationManager.Instance.GetLocalizedValue(DescTemplateHash);
        if (string.IsNullOrEmpty(template)) return "";

        // Chuyển đổi level thành index của mảng (Level 1 -> index 0)
        int index = Mathf.Max(0, currentUpgrade - 1);

        // Khởi tạo mảng biến động để chứa tất cả các tham số {0}, {1}, {2}, {3}...
        List<object> formatArgs = new List<object>();

        // 1. Thu thập tất cả các biến từ Tầng 1 (StaticModifiers: {0}, {1}...)
        if (StaticModifiers != null)
        {
            foreach (var sm in StaticModifiers)
            {
                if (sm.ModifyByUpgrade != null && sm.ModifyByUpgrade.Count > 0)
                {
                    formatArgs.Add(GetValueFromList(sm.ModifyByUpgrade, index));
                }
            }
        }

        // 2. Thu thập tất cả các biến từ Tầng 2 (CombatEvents: {2}, {3}...)
        if (CombatEvents != null)
        {
            foreach (var ce in CombatEvents)
            {
                if (ce.ModifyByUpgrade != null && ce.ModifyByUpgrade.Count > 0)
                {
                    formatArgs.Add(GetValueFromList(ce.ModifyByUpgrade, index));
                }
            }
        }

        // 3. Đổ dữ liệu vào chuỗi Template
        if (formatArgs.Count > 0)
        {
            try
            {
                // string.Format sẽ tự động map mảng args vào {0}, {1}, {2}, {3}...
                return string.Format(template, formatArgs.ToArray());
            }
            catch (FormatException)
            {
                // Fallback nếu template cũ chỉ dùng 2 tham số {0} và {1}
                try
                {
                    List<object> fallbackArgs = new List<object>();
                    if (StaticModifiers != null && StaticModifiers.Count > 0)
                        fallbackArgs.Add(GetValueFromList(StaticModifiers[0].ModifyByUpgrade, index));
                    if (CombatEvents != null && CombatEvents.Count > 0)
                        fallbackArgs.Add(GetValueFromList(CombatEvents[0].ModifyByUpgrade, index));

                    return string.Format(template, fallbackArgs.ToArray());
                }
                catch (FormatException e)
                {
                    // Bắt lỗi nếu Designer gõ sai ngoặc {0} trong file Text Localization
                    Debug.LogWarning($"[PassiveConfig] Lỗi Format Text tại Hash {DescTemplateHash}: {e.Message}");
                    return template;
                }
            }
        }

        // Trả về nguyên gốc nếu kỹ năng không có tham số nào biến thiên
        return template;
    }

    /// <summary>
    /// Hàm đọc giá trị an toàn, tự động khóa ở giá trị lớn nhất nếu Level vượt quá cấu hình
    /// </summary>
    private float GetValueFromList(List<float> list, int index)
    {
        if (list == null || list.Count == 0) return 0f;
        return list[Mathf.Min(index, list.Count - 1)];
    }
}