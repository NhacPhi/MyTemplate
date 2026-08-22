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
        if (context != null && context.Target == null && context.Source != null && context.Source.Target != null)
        {
            context.Target = context.Source.Target.GetComponent<Entity>();
            if (context.Source.Targets == null || context.Source.Targets.Count <= 1)
            {
                context.AddTag("SingleTarget");
            }
        }

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
        foreach (var rawTag in evtConfig.ConditionTags)
        {
            string requiredTag = rawTag?.Trim();
            if (string.IsNullOrEmpty(requiredTag)) continue;

            // 1. Kiểm tra điều kiện Máu của chủ sở hữu (Source HP)
            if (requiredTag.Equals("HP_Above_50", System.StringComparison.OrdinalIgnoreCase))
            {
                var stats = context.Source != null ? context.Source.GetCoreComponent<EntityStats>() : null;
                if (stats != null)
                {
                    var hp = stats.GetAttribute(AttributeType.Hp);
                    if (hp == null || (hp.Value / hp.MaxValue) <= 0.5f) return false;
                }
                continue;
            }

            if (requiredTag.Equals("HP_BelowOrEqual_50", System.StringComparison.OrdinalIgnoreCase) ||
                requiredTag.Equals("HP_Below_50", System.StringComparison.OrdinalIgnoreCase) ||
                requiredTag.Equals("HP_Under_50", System.StringComparison.OrdinalIgnoreCase))
            {
                var stats = context.Source != null ? context.Source.GetCoreComponent<EntityStats>() : null;
                if (stats != null)
                {
                    var hp = stats.GetAttribute(AttributeType.Hp);
                    if (hp == null || (hp.Value / hp.MaxValue) > 0.5f) return false;
                }
                continue;
            }

            // 2. Kiểm tra điều kiện Kỹ năng (IsSkill)
            if (requiredTag.Equals("IsSkill", System.StringComparison.OrdinalIgnoreCase))
            {
                if (context.HasTag("ActiveSkill") || context.HasTag("MajorSkill") || context.HasTag("UltimateSkill") || context.HasTag("IsSkill") || context.HasTag("Skill"))
                {
                    continue;
                }
                return false;
            }

            // 3. Kiểm tra điều kiện Target sống / chết
            if (requiredTag.Equals("TargetDead", System.StringComparison.OrdinalIgnoreCase))
            {
                if (context.Target != null && context.Target.GetCoreComponent<EntityStats>() != null && context.Target.GetCoreComponent<EntityStats>().IsDead)
                {
                    continue;
                }
                return false;
            }

            if (requiredTag.Equals("TargetAlive", System.StringComparison.OrdinalIgnoreCase))
            {
                if (context.Target != null && context.Target.GetCoreComponent<EntityStats>() != null && !context.Target.GetCoreComponent<EntityStats>().IsDead)
                {
                    continue;
                }
                return false;
            }

            // 4. Kiểm tra điều kiện Target có Debuff (TargetHasDebuff)
            if (requiredTag.Equals("TargetHasDebuff", System.StringComparison.OrdinalIgnoreCase) ||
                requiredTag.Equals("TargetDebuffed", System.StringComparison.OrdinalIgnoreCase))
            {
                var targetStats = context.Target != null ? context.Target.GetCoreComponent<EntityStats>() : null;
                if (targetStats != null)
                {
                    var defStat = targetStats.GetStat(StatType.DEF);
                    var spdStat = targetStats.GetStat(StatType.SPEED);
                    var atkStat = targetStats.GetStat(StatType.ATK);

                    bool hasNegativeMod = (defStat != null && defStat.Modifiers != null && defStat.Modifiers.Exists(m => m.Value < 0)) ||
                                          (spdStat != null && spdStat.Modifiers != null && spdStat.Modifiers.Exists(m => m.Value < 0)) ||
                                          (atkStat != null && atkStat.Modifiers != null && atkStat.Modifiers.Exists(m => m.Value < 0));

                    if (hasNegativeMod || context.HasTag("Debuff") || context.HasTag("Vulnerable") || context.HasTag("Poison") || context.HasTag("Burn") || context.HasTag("Stun"))
                    {
                        continue;
                    }
                }
                return false;
            }

            // 5. So khớp tag thông thường
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
        context.EventContextInfo = evtConfig.EffectParam.ToString();
        PassiveEffectFactory.ExecuteEffect(evtConfig.EffectId, finalTarget, effectValue, context);
    }

    /// <summary>
    /// Dịch chữ "Self", "Target" từ JSON ra thành Entity thực tế trên bàn cờ
    /// </summary>
    private static Entity ResolveTarget(string targetType, CombatContext context)
    {
        if (string.IsNullOrEmpty(targetType)) return context.Source;

        switch (targetType.ToLower())
        {
            case "self":
                return context.Source;
            case "target":
            case "enemy":
                return context.Target;
            default:
                return context.Source;
        }
    }
}
