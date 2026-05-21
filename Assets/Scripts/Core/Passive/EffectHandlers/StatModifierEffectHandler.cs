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
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        // Lấy stat tương ứng dựa vào _targetStat
        var stat = stats.GetStat(_targetStat);
        if (stat != null)
        {
            // Tùy thuộc vào code của bạn (dùng AddModifier hay cộng trực tiếp)
            // Ví dụ:
            // stat.AddModifier(new StatModifier(effectValue, _isPercentage ? StatModType.PercentAdd : StatModType.Flat));
            
            Debug.Log($"[Passive] Đã áp dụng {effectValue} ({(_isPercentage ? "%" : "Flat")}) vào chỉ số {_targetStat} của {target.name}");
        }
    }
}
