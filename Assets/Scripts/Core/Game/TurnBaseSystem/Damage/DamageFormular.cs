using Tech.Composite;
using UnityEngine;

public static class DamageFormular 
{
    private static System.Collections.Generic.Dictionary<int, int> prdAttackCounts = new System.Collections.Generic.Dictionary<int, int>();
    private const float DEF_CONSTANT = 400f;

   public static void DealDamge(DamageBonus damageBonus, Transform source, Transform target)
    {
        if(!source.TryGetComponent(out Tech.Composite.Core sourceCore)) return;
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

        float damageResult = sourceAtk.Value * damageBonus.DamageMultiplier + damageBonus.FlatValue;

        if (sourceSkill != null)
        {
            sourceSkill.ApplyAttackSkill(ref damageResult);
        }

        bool isCritical = false;
        var critRate = sourceStats.GetStat(StatType.CRIT_RATE);
        if (critRate != null && critRate.Value > 0)
        {
            int sourceId = source.GetInstanceID();
            if (!prdAttackCounts.TryGetValue(sourceId, out int attackCount))
            {
                attackCount = 1;
            }

            float p = critRate.Value / 100f;
            // PRD Constant Approximation (C ≈ P * P) for smoother distribution
            float c = (p >= 1f) ? 1f : (p * p);
            float currentCritChance = c * attackCount * 100f;

            if (currentCritChance >= 100f || UnityEngine.Random.Range(0f, 100f) < currentCritChance)
            {
                isCritical = true;
                prdAttackCounts[sourceId] = 1; // Reset PRD counter
                
                var critDmg = sourceStats.GetStat(StatType.CRIT_DMG);
                var critDmgRes = targetStats.GetStat(StatType.CRIT_DMG_RES);

                float attackerCritDmg = critDmg != null ? critDmg.Value : 0f;
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

        float effectiveDef = CalculateEffectiveDefense(sourceStats, targetStats);
        damageResult = Mathf.RoundToInt(damageResult * (DEF_CONSTANT / (DEF_CONSTANT + effectiveDef)));
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
        float damageResult = sourceAtk.Value * damageBonus.DamageMultiplier + damageBonus.FlatValue;

        if (sourceSkill != null)
        {
            sourceSkill.ApplyAttackSkill(ref damageResult);
        }

        var critRate = sourceStats.GetStat(StatType.CRIT_RATE);
        if (critRate != null && critRate.Value > 0)
        {
            var critDmg = sourceStats.GetStat(StatType.CRIT_DMG);
            var critDmgRes = targetStats.GetStat(StatType.CRIT_DMG_RES);

            float attackerCritDmg = critDmg != null ? critDmg.Value : 0f;
            float defenderCritRes = critDmgRes != null ? critDmgRes.Value : 0f;

            float totalCritDmgPercent = Mathf.Max(100f, 150f + attackerCritDmg - defenderCritRes);
            float critMultiplier = totalCritDmgPercent / 100f;

            float expectedCritFactor = 1f + (critRate.Value / 100f) * (critMultiplier - 1f);
            damageResult *= expectedCritFactor;
        }

        if (targetSkill != null)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        float effectiveDef = CalculateEffectiveDefense(sourceStats, targetStats);
        damageResult = damageResult * (DEF_CONSTANT / (DEF_CONSTANT + effectiveDef));
        return damageResult;
    }

    private static float CalculateEffectiveDefense(IDamagable sourceStats, IDamagable targetStats)
    {
        var targetDefStat = targetStats.GetStat(StatType.DEF);
        float baseDef = targetDefStat != null ? targetDefStat.Value : 0f;

        var penStat = sourceStats.GetStat(StatType.PENETRATION);
        var defShredStat = sourceStats.GetStat(StatType.DEF_SHRED);

        float penPercent = penStat != null ? penStat.Value : 0f; // VD: 20 -> 20%
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
