using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handler tích tầng Phòng Thủ (DEF) khi chịu sát thương (vd: Cửu Xỉ Đinh Ba psv_nine_toothed_rake).
/// Mỗi lần bị đánh nhận 1 tầng (+effectValue% DEF), tối đa maxStacks tầng.
/// Khi đạt maxStacks tầng, tự động hồi phục 10% HP Tối Đa.
/// </summary>
public class StackableDefEffectHandler : IEffectHandler
{
    private static readonly Dictionary<Entity, int> _entityStacks = new Dictionary<Entity, int>();
    private static readonly HashSet<Entity> _healedEntities = new HashSet<Entity>();

    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        var recipient = target;
        if (recipient == null && context != null) recipient = context.Source;
        if (recipient == null) return;

        var stats = recipient.GetCoreComponent<EntityStats>();
        if (stats == null || stats.IsDead) return;

        int maxStacks = 5;

        if (!_entityStacks.TryGetValue(recipient, out int currentStack))
        {
            currentStack = 0;
        }

        if (currentStack < maxStacks)
        {
            currentStack++;
            _entityStacks[recipient] = currentStack;

            var defStat = stats.GetStat(StatType.DEF);
            if (defStat != null)
            {
                defStat.AddModifier(new Modifier(effectValue / 100f, ModifyType.Percent));
            }

            // Khi đạt mốc maxStacks tối đa -> Hồi phục 10% HP Tối Đa
            if (currentStack >= maxStacks && !_healedEntities.Contains(recipient))
            {
                _healedEntities.Add(recipient);
                var hpAttr = stats.GetAttribute(AttributeType.Hp);
                if (hpAttr != null)
                {
                    float healAmount = hpAttr.MaxValue * 0.10f;
                    stats.HealingHP(healAmount);
                }
            }
        }
    }
}
