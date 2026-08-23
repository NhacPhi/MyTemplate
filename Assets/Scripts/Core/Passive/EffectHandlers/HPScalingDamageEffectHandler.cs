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

        float maxHp = 0f;
        var hpStat = stats.GetStat(StatType.HP);
        if (hpStat != null && hpStat.Value > 0)
        {
            maxHp = hpStat.Value;
        }
        else
        {
            var hpAttr = stats.GetAttribute(AttributeType.Hp);
            if (hpAttr != null && hpAttr.MaxValue > 0)
            {
                maxHp = hpAttr.MaxValue;
            }
            else if (hpAttr != null && hpAttr.Value > 0)
            {
                maxHp = hpAttr.Value;
            }
        }

        // effectValue là phần trăm (Ví dụ: 30 = 30% Max HP)
        float additionalFlatDamage = (effectValue / 100f) * maxHp;

        if (context.DamageBonus.HasValue)
        {
            var bonus = context.DamageBonus.Value;
            bonus.FlatValue += additionalFlatDamage;
            context.DamageBonus = bonus;
        }

        Debug.Log($"[HPScalingDamage] {source.name} thi triển Tuyệt Kỹ: Kích hoạt +{effectValue}% Máu Tối Đa (MaxHP = {maxHp:N0}) -> Sát thương phẳng cộng thêm: +{additionalFlatDamage:N0} Flat DMG!");
    }
}
