using Tech.Composite;
using UnityEngine;

public static class DamageFormular 
{
    private static System.Collections.Generic.Dictionary<int, int> prdAttackCounts = new System.Collections.Generic.Dictionary<int, int>();

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
                // LoL Base Crit Damage is 175%, plus any bonus Crit Dmg
                float critMultiplier = (175f + (critDmg != null ? critDmg.Value : 0f)) / 100f;
                damageResult *= critMultiplier;
            }
            else
            {
                prdAttackCounts[sourceId] = attackCount + 1; // Increment PRD counter
            }
        }

        var targetDef = targetStats.GetStat(StatType.DEF);

        if (targetSkill)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        damageResult = Mathf.RoundToInt(damageResult * (100f / (100 + targetDef.Value)));
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

        var targetDef = targetStats.GetStat(StatType.DEF);
        if (targetSkill != null)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        damageResult = damageResult * (100f / (100 + targetDef.Value));
        return damageResult;
    }



    private static void GetStatsAndSkillSystem(Tech.Composite.Core core,
        out IDamagable entityStats, out EntitySkill entitySkill)
    {
        entityStats = core.GetCoreComponent<IDamagable>();
        //Need Change SkillCharacter To Interface
        entitySkill = core.GetCoreComponent<EntitySkill>();
    }
}
