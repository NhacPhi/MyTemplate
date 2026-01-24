using Tech.Composite;
using UnityEngine;

public static class DamageFormular 
{
   public static void DealDamge(DamageBonus damageBonus, Transform source, Transform target)
    {
        if(!source.TryGetComponent(out Tech.Composite.Core sourceCore)) return;
        if (!target.TryGetComponent(out Tech.Composite.Core targetCore)) return;

        DealDamage(damageBonus, sourceCore, targetCore);
    }
    //Damage Not Aplly Any Skill = SourceATk* Multiplier + FlatValue - TargetDef
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

        var targetDef = targetStats.GetStat(StatType.DEF);

        if (targetSkill)
        {
            targetSkill.ApplyDefenseSkill(ref damageResult, source.transform);
        }

        damageResult = Mathf.RoundToInt(damageResult * (100f / (100 + targetDef.Value)));
        targetStats.TakeDamage(damageResult, source.transform);
        UIEvent.DamagePopup(damageResult, target.transform.position, false);
    }


    private static void GetStatsAndSkillSystem(Tech.Composite.Core core,
        out IDamagable entityStats, out EntitySkill entitySkill)
    {
        entityStats = core.GetCoreComponent<IDamagable>();
        //Need Change Skill To Interface
        entitySkill = core.GetCoreComponent<EntitySkill>();
    }
}
