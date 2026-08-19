using UnityEngine;

/// <summary>
/// Handler xử lý hiệu ứng tăng Xuyên Giáp (% Armor Penetration).
/// Được kích hoạt trong sự kiện OnBeforeDealDamage.
/// </summary>
public class ArmorPenetrationEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        // effectValue là % xuyên giáp (Ví dụ: 30 = 30% Penetration)
        if (context.DamageBonus.HasValue)
        {
            var bonus = context.DamageBonus.Value;
            bonus.PenetrationBonus += effectValue;
            context.DamageBonus = bonus;
        }
    }
}
