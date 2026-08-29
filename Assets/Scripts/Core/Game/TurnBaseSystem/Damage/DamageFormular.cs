using System.Collections.Generic;
using System.Linq;
using Tech.Composite;
using UnityEngine;

public static class DamageFormular 
{
    private static System.Collections.Generic.Dictionary<int, int> prdAttackCounts = new System.Collections.Generic.Dictionary<int, int>();
    private const float DEF_CONSTANT = 1000f;

    public static void DealDamge(DamageBonus damageBonus, Transform source, Transform target)
    {
        DealDamage(damageBonus, source, target);
    }

    public static void DealDamage(DamageBonus damageBonus, Transform source, Transform target)
    {
        if (source == null || target == null) return;
        if (!source.TryGetComponent(out Tech.Composite.Core sourceCore)) return;
        if (!target.TryGetComponent(out Tech.Composite.Core targetCore)) return;

        DealDamage(damageBonus, sourceCore, targetCore);
    }
    //Damage Not Aplly Any SkillCharacter = SourceATk* Multiplier + FlatValue - TargetDef
    public static void DealDamage(DamageBonus damageBonus, Tech.Composite.Core source, Tech.Composite.Core target)
    {
        GetStatsAndSkillSystem(source, out var sourceStats, out var sourceSkill);
        GetStatsAndSkillSystem(target, out var targetStats, out var targetSkill);

        if (sourceStats == null || targetStats == null) return;

        var sourceAtk = sourceStats.GetStat(StatType.ATK);

        float damageResult = (sourceAtk.Value * damageBonus.DamageMultiplier) + damageBonus.FlatValue;

        // Cơ chế Diệt Giáp Ảo (Shield Breaker): x2 Sát Thương khi mục tiêu đang có Giáp Ảo
        if (damageBonus.Tags != null && (damageBonus.Tags.Contains("ShieldBreaker") || damageBonus.Tags.Contains("DoubleDamageOnShield")))
        {
            var shieldAttr = targetStats.GetAttribute(AttributeType.Shield);
            if (shieldAttr != null && shieldAttr.Value > 0)
            {
                damageResult *= 2f;
            }
        }

        // Cơ chế Cặp Song Sát: Chỉ khi đòn đánh có Tag Song Sát (Kim Giác) VÀ mục tiêu đang dính Ngân Ấn
        if (damageBonus.Tags != null && (damageBonus.Tags.Contains("SilverMarkSynergy") || damageBonus.Tags.Contains("SilverMarkBonus")))
        {
            var targetEffectHolder = target.GetCoreComponent<StatsController>();
            if (targetEffectHolder != null && targetEffectHolder.StatusEffect != null)
            {
                bool hasSilverMark = targetEffectHolder.StatusEffect.Any(e => e.Data != null && (e.Data.Type == EffectType.SilverMark || e.ID == "EFF_SilverMark_def"));
                if (hasSilverMark)
                {
                    damageResult *= 1.20f;
                    UIEvent.TextPopup?.Invoke("Song Sát +20%!", target.transform.position + Vector3.up * 1.5f, new Color(1f, 0.85f, 0.2f));
                }
            }
        }

        if (sourceSkill != null)
        {
            sourceSkill.ApplyAttackSkill(ref damageResult);
        }

        bool isCritical = false;
        var critRate = sourceStats.GetStat(StatType.CRIT_RATE);
        float baseCritRate = (critRate != null ? critRate.Value : 0f) + damageBonus.CritRateBonus;
        if (baseCritRate > 0)
        {
            int sourceId = source.GetInstanceID();
            if (!prdAttackCounts.TryGetValue(sourceId, out int attackCount))
            {
                attackCount = 1;
            }

            float p = baseCritRate / 100f;
            // PRD Constant Approximation (C ≈ P * P) for smoother distribution
            float c = (p >= 1f) ? 1f : (p * p);
            float currentCritChance = c * attackCount * 100f;

            if (currentCritChance >= 100f || UnityEngine.Random.Range(0f, 100f) < currentCritChance)
            {
                isCritical = true;
                prdAttackCounts[sourceId] = 1; // Reset PRD counter
                
                var critDmg = sourceStats.GetStat(StatType.CRIT_DMG);
                var critDmgRes = targetStats.GetStat(StatType.CRIT_DMG_RES);

                float attackerCritDmg = (critDmg != null ? critDmg.Value : 0f) + damageBonus.CritDmgBonus;
                float defenderCritRes = critDmgRes != null ? critDmgRes.Value : 0f;

                // Cách 1 (Chuẩn Honkai Star Rail / Direct Subtraction):
                // Tổng % Crit DMG = 150% (Base Gốc) + Crit DMG Kẻ Tấn Công - Kháng Bạo Kích Mục Tiêu
                // Tối thiểu = 100% (Đòn Crit luôn gây ít nhất 100% bằng đòn đánh thường)
                float totalCritDmgPercent = Mathf.Max(100f, 150f + attackerCritDmg - defenderCritRes);
                float critMultiplier = totalCritDmgPercent / 100f;

                damageResult = damageResult * critMultiplier;
            }
            else
            {
                prdAttackCounts[sourceId] = attackCount + 1; // Increment PRD counter
            }
        }

        if (targetSkill)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        // Giảm sát thương nhận vào từ nội tại bảo vật Lung Linh Bảo Tháp (psv_linglong_pagoda)
        // Cơ chế (Option A Cân Bằng): Giảm 15% sát thương ở đòn đầu tiên; các đòn tiếp theo có 40% tỉ lệ kích hoạt giảm 10% sát thương.
        var targetPassive = target.GetComponent<EntityPassive>();
        if (targetPassive != null && targetPassive.ActivePassives != null)
        {
            foreach (var p in targetPassive.ActivePassives)
            {
                if (p.Config != null && p.Config.ID == "psv_linglong_pagoda")
                {
                    bool triggerReduction = false;
                    float reductionFactor = 1f;

                    if (p.StackCount == 0)
                    {
                        // Đòn đánh đầu tiên: Chắc chắn 100% kích hoạt giảm 15%
                        triggerReduction = true;
                        reductionFactor = 0.85f; // Giảm 15%
                        p.StackCount = 1;
                    }
                    else
                    {
                        // Các đòn tiếp theo: 40% tỉ lệ kích hoạt giảm 10%
                        if (UnityEngine.Random.Range(0f, 100f) < 40f)
                        {
                            triggerReduction = true;
                            reductionFactor = 0.90f; // Giảm 10%
                        }
                    }

                    if (triggerReduction)
                    {
                        damageResult *= reductionFactor;

                        // Hiển thị Text Popup thông qua LocalizationManager (không hardcode)
                        string popupText = LocalizationManager.Instance != null 
                            ? LocalizationManager.Instance.GetLocalizedValue("STR_DAMAGE_REDUCED") 
                            : "";
                        if (string.IsNullOrEmpty(popupText) || popupText == "STR_DAMAGE_REDUCED") popupText = "Giảm Sát Thương!";

                        UIEvent.TextPopup?.Invoke(popupText, target.transform.position + Vector3.up * 1.5f, new Color(0.3f, 0.85f, 1f));
                    }
                    break;
                }
            }
        }

        float effectiveDef = CalculateEffectiveDefense(sourceStats, targetStats, damageBonus.PenetrationBonus);
        damageResult = Mathf.RoundToInt(damageResult * (DEF_CONSTANT / (DEF_CONSTANT + effectiveDef)));

        if (isCritical)
        {
            if (damageBonus.Tags == null) damageBonus.Tags = new HashSet<string>();
            damageBonus.Tags.Add("IsCritical");
            damageBonus.Tags.Add("Critical");
        }

        targetStats.TakeDamage(damageResult, source.transform, damageBonus.Tags);
        UIEvent.DamagePopup(damageResult, target.transform.position, isCritical);
    }

    // Tính toán sát thương nháp (không thực sự gây sát thương) để dự đoán mục tiêu có chết không
    public static float SimulateDamage(DamageBonus damageBonus, Tech.Composite.Core source, Tech.Composite.Core target)
    {
        GetStatsAndSkillSystem(source, out var sourceStats, out var sourceSkill);
        GetStatsAndSkillSystem(target, out var targetStats, out var targetSkill);

        if (sourceStats == null || targetStats == null) return 0;

        var sourceAtk = sourceStats.GetStat(StatType.ATK);
        float damageResult = (sourceAtk.Value * damageBonus.DamageMultiplier) + damageBonus.FlatValue;

        if (damageBonus.Tags != null && (damageBonus.Tags.Contains("ShieldBreaker") || damageBonus.Tags.Contains("DoubleDamageOnShield")))
        {
            var shieldAttr = targetStats.GetAttribute(AttributeType.Shield);
            if (shieldAttr != null && shieldAttr.Value > 0)
            {
                damageResult *= 2f;
            }
        }

        if (damageBonus.Tags != null && (damageBonus.Tags.Contains("SilverMarkSynergy") || damageBonus.Tags.Contains("SilverMarkBonus")))
        {
            var targetEffectHolder = target.GetCoreComponent<StatsController>();
            if (targetEffectHolder != null && targetEffectHolder.StatusEffect != null)
            {
                bool hasSilverMark = targetEffectHolder.StatusEffect.Any(e => e.Data != null && (e.Data.Type == EffectType.SilverMark || e.ID == "EFF_SilverMark_def"));
                if (hasSilverMark)
                {
                    damageResult *= 1.20f;
                }
            }
        }

        if (sourceSkill != null)
        {
            sourceSkill.ApplyAttackSkill(ref damageResult);
        }

        var critRate = sourceStats.GetStat(StatType.CRIT_RATE);
        float baseCritRate = (critRate != null ? critRate.Value : 0f) + damageBonus.CritRateBonus;
        if (baseCritRate > 0)
        {
            var critDmg = sourceStats.GetStat(StatType.CRIT_DMG);
            var critDmgRes = targetStats.GetStat(StatType.CRIT_DMG_RES);

            float attackerCritDmg = (critDmg != null ? critDmg.Value : 0f) + damageBonus.CritDmgBonus;
            float defenderCritRes = critDmgRes != null ? critDmgRes.Value : 0f;

            float totalCritDmgPercent = Mathf.Max(100f, 150f + attackerCritDmg - defenderCritRes);
            damageResult = damageResult * (totalCritDmgPercent / 100f);
        }

        if (targetSkill)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        // Giảm sát thương nhận vào từ nội tại bảo vật Lung Linh Bảo Tháp (psv_linglong_pagoda)
        var simTargetPassive = target.GetComponent<EntityPassive>();
        if (simTargetPassive != null && simTargetPassive.ActivePassives != null)
        {
            foreach (var p in simTargetPassive.ActivePassives)
            {
                if (p.Config != null && p.Config.ID == "psv_linglong_pagoda")
                {
                    if (p.StackCount == 0)
                    {
                        damageResult *= 0.85f; // Giảm 15% đòn đầu
                    }
                    else
                    {
                        damageResult *= 0.96f; // Kỳ vọng giảm 40% * 10% = 4%
                    }
                    break;
                }
            }
        }

        float effectiveDef = CalculateEffectiveDefense(sourceStats, targetStats, damageBonus.PenetrationBonus);
        damageResult = Mathf.RoundToInt(damageResult * (DEF_CONSTANT / (DEF_CONSTANT + effectiveDef)));
        return damageResult;
    }

    private static float CalculateEffectiveDefense(IDamagable sourceStats, IDamagable targetStats, float penetrationBonus = 0f)
    {
        var targetDefStat = targetStats.GetStat(StatType.DEF);
        float baseDef = targetDefStat != null ? targetDefStat.Value : 0f;

        var penStat = sourceStats.GetStat(StatType.PENETRATION);
        var defShredStat = sourceStats.GetStat(StatType.DEF_SHRED);

        float penPercent = Mathf.Clamp((penStat != null ? penStat.Value : 0f) + penetrationBonus, 0f, 100f); // Giới hạn tối đa 100%
        float defShredFlat = defShredStat != null ? defShredStat.Value : 0f; // VD: 150 -> 150 DEF Phẳng

        // Bước 1: Xuyên giáp % (PEN) tính trước
        float defAfterPen = baseDef * (1f - (penPercent / 100f));
        // Bước 2: Trừ giáp phẳng (DEF_SHRED) trừ trực tiếp chỉ số giáp
        float effectiveDef = Mathf.Max(0f, defAfterPen - defShredFlat);

        return effectiveDef;
    }



    private static void GetStatsAndSkillSystem(Tech.Composite.Core core,
        out IDamagable entityStats, out EntitySkill entitySkill)
    {
        entityStats = core.GetCoreComponent<IDamagable>();
        //Need Change SkillCharacter To Interface
        entitySkill = core.GetCoreComponent<EntitySkill>();
    }
}
