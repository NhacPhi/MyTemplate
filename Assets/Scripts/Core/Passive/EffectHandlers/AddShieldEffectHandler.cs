using UnityEngine;

public class AddShieldEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        float maxHp = (stats.GetStat(StatType.HP) != null && stats.GetStat(StatType.HP).Value > 0)
            ? stats.GetStat(StatType.HP).Value
            : 1000f;

        // effectValue là phần trăm (ví dụ 5 = 5% Max HP)
        float shieldAmount = (effectValue / 100f) * maxHp;

        // Giới hạn cộng dồn tối đa 5 tầng = 25% Max HP
        var shieldAttr = stats.GetAttribute(AttributeType.Shield);
        float currentShield = shieldAttr != null ? shieldAttr.Value : 0f;
        float maxShieldCap = maxHp * 0.25f;

        if (currentShield + shieldAmount > maxShieldCap)
        {
            shieldAmount = Mathf.Max(0f, maxShieldCap - currentShield);
        }

        if (shieldAmount > 0f)
        {
            stats.BuffShield(shieldAmount);
        }
    }
}
