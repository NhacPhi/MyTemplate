using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý và phân phối việc thực thi cho tất cả các loại Effect trong game.
/// Áp dụng mẫu thiết kế Strategy + Factory.
/// </summary>
public static class PassiveEffectFactory
{
    private static readonly Dictionary<string, IEffectHandler> _handlers = new Dictionary<string, IEffectHandler>()
    {
        { "Eff_HealPercentage", new HealPercentageEffectHandler() },
        { "Eff_Lifesteal", new LifestealEffectHandler() },
        { "Eff_AddShield", new AddShieldEffectHandler() },

        // ------ SỬ DỤNG CHUNG CLASS CHO CÁC CHỈ SỐ ------
        { "Eff_ReduceDefense", new StatModifierEffectHandler(StatType.DEF, true) },
        { "Eff_IncreaseAttack", new StatModifierEffectHandler(StatType.ATK, true) },
        { "Eff_ReduceSpeed", new StatModifierEffectHandler(StatType.SPEED, true) },
        { "Eff_IncreaseCritRate", new StatModifierEffectHandler(StatType.CRIT_RATE, false) },
        { "Eff_IncreaseCritDmg", new StatModifierEffectHandler(StatType.CRIT_DMG, false) },

        // ------ CÁC HANDLER SÁT THƯƠNG ĐẶC BIỆT ------
        { "Eff_HPScalingDamage", new HPScalingDamageEffectHandler() },
        { "Eff_ArmorPenetration", new ArmorPenetrationEffectHandler() },
        { "Eff_CounterAttack", new CounterAttackEffectHandler() },
        { "Eff_Action_Point_Boost", new ActionAdvanceEffectHandler() },
        { "Eff_Stackable_DEF_Buff", new StackableDefEffectHandler() },
        { "Eff_Reduce_Cooldown", new CooldownReductionEffectHandler() },
        { "Eff_FollowUp_BasicAttack", new FollowUpAttackEffectHandler() }
    };

    public static void ExecuteEffect(string effectId, Entity target, float effectValue, CombatContext context)
    {
        if (_handlers.TryGetValue(effectId, out var handler))
        {
            handler.Execute(target, effectValue, context);
        }
        else
        {
            Debug.LogWarning($"[PassiveEffectFactory] Chưa có Handler cho EffectId: {effectId}. Hãy tạo class mới kế thừa IEffectHandler và đăng ký vào PassiveEffectFactory!");
        }
    }
}
