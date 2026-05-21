using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PassiveEventListener
{
    /// <summary>
    /// So khớp (Matching) và Thực thi (Execution)
    /// </summary>
    public static void EvaluateAndExecute(CombatEventConfig evtConfig, int passiveLevel, CombatContext context)
    {
        bool matched = IsConditionMatched(evtConfig, context);
        Debug.Log($"[PassiveSystem] Kiểm tra Passive '{evtConfig.EffectId}' - Cần tag '{evtConfig.ConditionFilter}' - Có các tags: [{(context.Tags != null ? string.Join(", ", context.Tags) : "none")}] - Kết quả Match: {matched}");

        // 1. So khớp Tags (Condition Matching)
        if (!matched)
        {
            return; // Context không thỏa mãn điều kiện yêu cầu -> Bỏ qua
        }

        // 2. Thực thi (Execution) nếu đủ điều kiện
        ExecuteEffect(evtConfig, passiveLevel, context);
    }

    /// <summary>
    /// Lấy RequiredConditions từ JSON đọ với EventTags thực tế vừa diễn ra.
    /// Nếu thực tế có đủ các tag mà JSON yêu cầu -> Kích hoạt!
    /// </summary>
    private static bool IsConditionMatched(CombatEventConfig evtConfig, CombatContext context)
    {
        // Nếu config không yêu cầu tag gì, coi như luôn luôn khớp (thỏa mãn)
        if (evtConfig.ConditionTags == null || evtConfig.ConditionTags.Count == 0)
        {
            return true; 
        }

        // Kiểm tra xem Context thực tế có chứa TẤT CẢ các Tags mà JSON yêu cầu không (Logic AND)
        foreach (var requiredTag in evtConfig.ConditionTags)
        {
            if (!context.HasTag(requiredTag))
            {
                return false; // Thiếu 1 tag yêu cầu -> Match thất bại
            }
        }

        return true; // Có đủ tất cả các tags
    }

    /// <summary>
    /// Xử lý áp dụng Effect lên Target dựa theo Config
    /// </summary>
    private static void ExecuteEffect(CombatEventConfig evtConfig, int passiveLevel, CombatContext context)
    {
        int levelIndex = Mathf.Max(0, passiveLevel - 1);
        float effectValue = 0f;
        
        if (evtConfig.ModifyByUpgrade != null && evtConfig.ModifyByUpgrade.Count > 0)
        {
             effectValue = evtConfig.ModifyByUpgrade[Mathf.Min(levelIndex, evtConfig.ModifyByUpgrade.Count - 1)];
        }

        // 3. Phân tích Mục Tiêu (Target Resolution) dựa vào cấu trúc JSON
        Entity finalTarget = ResolveTarget(evtConfig.Target, context);
        if (finalTarget == null) return;

        var stats = finalTarget.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        // 4. Ủy quyền (Delegate) cho EffectFactory xử lý logic thực thi
        PassiveEffectFactory.ExecuteEffect(evtConfig.EffectId, finalTarget, effectValue, context);
    }

    /// <summary>
    /// Dịch chữ "Self", "Target" từ JSON ra thành Entity thực tế trên bàn cờ
    /// </summary>
    private static Entity ResolveTarget(string targetType, CombatContext context)
    {
        if (string.IsNullOrEmpty(targetType)) return context.Source; // Mặc định là Source (Self)

        switch (targetType)
        {
            case "Self":
                return context.Source;
            case "Target":
                return context.Target;
            // Ở đây bạn có thể mở rộng cho "AllAllies", "LowestHPAlly"... (trả về List<Entity> nếu cần AoE)
            default:
                return context.Source;
        }
    }
}
