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
        { "Eff_IncreaseCritRate", new StatModifierEffectHandler(StatType.CRIT_RATE, false) }
        
        // Thêm các Handler mới vào đây (VD: "Poison", "Stun", "ActionAdvance"...)
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
