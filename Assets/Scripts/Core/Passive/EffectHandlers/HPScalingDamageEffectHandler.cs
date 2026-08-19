using UnityEngine;

/// <summary>
/// Handler xử lý hiệu ứng tăng Sát thương Gốc dựa theo % Máu Tối Đa (Zhongli Style).
/// Được kích hoạt trong sự kiện OnBeforeDealDamage.
/// </summary>
public class HPScalingDamageEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        Entity source = context.Source != null ? context.Source : target;
        if (source == null) return;

        var stats = source.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        float maxHp = (stats.GetStat(StatType.HP) != null && stats.GetStat(StatType.HP).Value > 0)
            ? stats.GetStat(StatType.HP).Value
            : 0f;

        // effectValue là phần trăm (Ví dụ: 30 = 30% Max HP)
        float additionalFlatDamage = (effectValue / 100f) * maxHp;

        if (context.DamageBonus.HasValue)
        {
            var bonus = context.DamageBonus.Value;
            bonus.FlatValue += additionalFlatDamage;
            context.DamageBonus = bonus;
        }
    }
}
