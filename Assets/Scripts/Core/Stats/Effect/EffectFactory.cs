using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Logger;

public class EffectFactory 
{
    public static StatusEffect CreateEffect(string efectfID, EffectConfig effectData, StatsController target)
    {
        if (effectData == null) return null;

        switch (effectData.Type)
        {
            case EffectType.None:
                return null;
            case EffectType.Poison:
                return new PoisonEffect(efectfID, effectData, target);
            case EffectType.Stun:
                return new StunEffect(efectfID, effectData, target); ;
            case EffectType.StatBuff:
            case EffectType.StatDebuff:
                return new StatBuffEffect(efectfID, effectData, target);

            case EffectType.ResetDebuff:
                return null;
            default:
                LogCommon.LogError($"[EffectFactory] Không tìm thấy class nào cho Effect Type: {effectData.Type}");
                return null;
        }   
    }
}
