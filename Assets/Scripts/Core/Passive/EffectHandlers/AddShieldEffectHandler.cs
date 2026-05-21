using UnityEngine;

public class AddShieldEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        stats.BuffShield(effectValue);
    }
}
