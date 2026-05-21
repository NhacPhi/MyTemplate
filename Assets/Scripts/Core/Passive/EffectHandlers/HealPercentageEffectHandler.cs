using UnityEngine;

public class HealPercentageEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        float healAmount = stats.GetAttribute(AttributeType.Hp).MaxValue * (effectValue / 100f);
        stats.HealingHP(healAmount);
    }
}
