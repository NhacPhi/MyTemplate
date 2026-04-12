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

    [JsonProperty("condition_filter")]
    public string ConditionFilter;

    [JsonProperty("effect_param")]
    public float EffectParam;

    [JsonProperty("internal_cooldown")]
    public int InternalCooldown;
}

// --- MASTER CLASS: QUẢN LÝ KỸ NĂNG ---
[Serializable]
public class PassiveConfig
{
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

        // Khởi tạo mảng biến động để chứa các tham số {0}, {1}, {2}...
        List<object> formatArgs = new List<object>();

        // 1. Thu thập biến cho Tầng 1 (Thường là biến {0})
        if (StaticModifiers != null && StaticModifiers.Count > 0)
        {
            // Mặc định lấy dòng Modifiers đầu tiên làm mốc. 
            // (Ví dụ: Dù có tăng ATK và DEF, chúng thường mang chung một mảng giá trị)
            formatArgs.Add(GetValueFromList(StaticModifiers[0].ModifyByUpgrade, index));
        }

        // 2. Thu thập biến cho Tầng 2 (Thường là biến {1}, hoặc đẩy lên thành {0} nếu không có Tầng 1)
        if (CombatEvents != null && CombatEvents.Count > 0)
        {
            formatArgs.Add(GetValueFromList(CombatEvents[0].ModifyByUpgrade, index));
        }

        // 3. Đổ dữ liệu vào chuỗi Template
        if (formatArgs.Count > 0)
        {
            try
            {
                // string.Format sẽ tự động map mảng args vào {0}, {1}...
                return string.Format(template, formatArgs.ToArray());
            }
            catch (FormatException e)
            {
                // Bắt lỗi nếu Designer gõ sai ngoặc {0} trong file Text Localization
                Debug.LogWarning($"[PassiveConfig] Lỗi Format Text tại Hash {DescTemplateHash}: {e.Message}");
                return template;
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