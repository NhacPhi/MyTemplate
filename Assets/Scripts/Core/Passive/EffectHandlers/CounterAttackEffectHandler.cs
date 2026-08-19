using UnityEngine;

/// <summary>
/// Handler xử lý phản đòn (Counter-Attack) khi bị tấn công.
/// Gây sát thương dựa trên % ATK của Source lên kẻ vừa tấn công.
/// </summary>
public class CounterAttackEffectHandler : IEffectHandler
{
    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        if (target == null || context == null || context.Source == null) return;

        // Chống vòng lặp phản đòn vô tận
        if (context.Tags != null && context.Tags.Contains("CounterAttack")) return;

        var source = context.Source;
        var sourceStats = source.GetCoreComponent<EntityStats>();
        var targetStats = target.GetCoreComponent<EntityStats>();

        if (sourceStats == null || sourceStats.IsDead || targetStats == null || targetStats.IsDead) return;

        // Kiểm tra tỷ lệ xác suất phản kích (effectValue là % cơ hội, ví dụ: 50 -> 50% cơ hội)
        float chance = effectValue > 0 ? effectValue : 100f;
        if (chance < 100f)
        {
            float roll = Random.Range(0f, 100f);
            if (roll >= chance)
            {
                return; // Không kích hoạt phản đòn
            }
        }

        DamageBonus bonus = new DamageBonus
        {
            DamageMultiplier = 1.0f,
            CritRateBonus = 0,
            CritDmgBonus = 0,
            PenetrationBonus = 0
        };
        bonus.AddTag("CounterAttack");

        DamageFormular.DealDamage(bonus, source, target);
    }
}
