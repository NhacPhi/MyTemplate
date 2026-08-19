using UnityEngine;

public class StatModifierEffectHandler : IEffectHandler
{
    private readonly StatType _targetStat;
    private readonly bool _isPercentage;

    public StatModifierEffectHandler(StatType targetStat, bool isPercentage = true)
    {
        _targetStat = targetStat;
        _isPercentage = isPercentage;
    }

    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        // 1. Nếu đang trong luồng tính toán đòn đánh (OnBeforeDealDamage), cập nhật trực tiếp vào DamageBonus
        if (context != null && context.DamageBonus.HasValue)
        {
            var bonus = context.DamageBonus.Value;
            switch (_targetStat)
            {
                case StatType.CRIT_DMG:
                    bonus.CritDmgBonus += effectValue;
                    context.DamageBonus = bonus;
                    return;
                case StatType.CRIT_RATE:
                    bonus.CritRateBonus += effectValue;
                    context.DamageBonus = bonus;
                    return;
                case StatType.ATK:
                    bonus.DamageMultiplier += _isPercentage ? (effectValue / 100f) : 0f;
                    bonus.FlatValue += !_isPercentage ? effectValue : 0f;
                    context.DamageBonus = bonus;
                    return;
            }
        }

        // 2. Mặc định áp dụng Modifier lên EntityStats của mục tiêu
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        var stat = stats.GetStat(_targetStat);
        if (stat != null)
        {
            var modType = _isPercentage ? ModifyType.Percent : ModifyType.Flat;
            float val = _isPercentage ? (effectValue / 100f) : effectValue;
            stat.AddModifier(new Modifier(val, modType));
            
            Debug.Log($"[Passive] Đã áp dụng {effectValue} ({(_isPercentage ? "%" : "Flat")}) vào chỉ số {_targetStat} của {target.name}");
        }
    }
}
