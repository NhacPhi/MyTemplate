using UnityEngine;

public class StatModifierEffectHandler : IEffectHandler
{
    private readonly StatType _targetStat;
    private readonly bool _isPercentage;
    private readonly bool _isReduction;

    public StatModifierEffectHandler(StatType targetStat, bool isPercentage = true, bool isReduction = false)
    {
        _targetStat = targetStat;
        _isPercentage = isPercentage;
        _isReduction = isReduction;
    }

    public void Execute(Entity target, float effectValue, CombatContext context)
    {
        // 1. Kiểm tra tỉ lệ xác suất kích hoạt nếu có EventContextInfo (VD: "60" -> 60% xác suất)
        if (context != null && !string.IsNullOrEmpty(context.EventContextInfo))
        {
            if (float.TryParse(context.EventContextInfo, out float procChance) && procChance > 0f && procChance < 100f)
            {
                float roll = Random.Range(0f, 100f);
                if (roll > procChance)
                {
                    Debug.Log($"[Passive] Xác suất {procChance}% không kích hoạt {_targetStat} modifier (Roll: {roll:F1})");
                    return;
                }
            }
        }

        // 2. Nếu đang trong luồng tính toán đòn đánh (OnBeforeDealDamage), cập nhật trực tiếp vào DamageBonus
        if (context != null && context.DamageBonus.HasValue)
        {
            var bonus = context.DamageBonus.Value;
            float modSign = _isReduction ? -1f : 1f;
            switch (_targetStat)
            {
                case StatType.CRIT_DMG:
                    bonus.CritDmgBonus += effectValue * modSign;
                    context.DamageBonus = bonus;
                    return;
                case StatType.CRIT_RATE:
                    bonus.CritRateBonus += effectValue * modSign;
                    context.DamageBonus = bonus;
                    return;
                case StatType.ATK:
                    bonus.DamageMultiplier += _isPercentage ? ((effectValue * modSign) / 100f) : 0f;
                    bonus.FlatValue += !_isPercentage ? (effectValue * modSign) : 0f;
                    context.DamageBonus = bonus;
                    return;
            }
        }

        // 3. Áp dụng StatusEffect vào EntityStats để kích hoạt hiển thị Icon trên UI & bộ đếm lượt
        var stats = target.GetCoreComponent<EntityStats>();
        if (stats == null) return;

        int duration = 2; // Mặc định 2 hiệp
        var effectData = new EffectConfig
        {
            Type = _isReduction ? EffectType.StatDebuff : EffectType.StatBuff,
            TargetStat = _targetStat,
            ModifyType = _isPercentage ? ModifyType.Percent : ModifyType.Flat,
            Duration = duration,
            MaxStack = 1,
            Value = _isReduction ? -(int)effectValue : (int)effectValue
        };

        string effectId = $"EFF_{(_isReduction ? "DEBUFF" : "BUFF")}_{_targetStat}";
        var statusEffect = new StatBuffEffect(effectId, effectData, stats);
        int turnId = BattleManager.Instance != null ? BattleManager.Instance.GlobalTurnID : 0;
        stats.ApplyEffect(statusEffect, turnId);

        // Hiển thị Text Popup trực quan trên đầu mục tiêu trong trận đấu
        string sign = _isReduction ? "-" : "+";
        string unit = _isPercentage ? "%" : "";
        string statName = _targetStat switch
        {
            StatType.DEF => "Giảm Thủ",
            StatType.SPEED => "Giảm Tốc",
            StatType.ATK => "Tấn Công",
            StatType.CRIT_RATE => "Tỉ Lệ Bạo",
            StatType.CRIT_DMG => "ST Bạo",
            _ => _targetStat.ToString()
        };
        Color popupColor = _isReduction ? new Color(0.95f, 0.35f, 0.35f) : new Color(0.35f, 0.95f, 0.45f);
        UIEvent.TextPopup?.Invoke($"{sign}{effectValue}{unit} {statName}!", target.transform.position + Vector3.up * 1.5f, popupColor);
        
        Debug.Log($"[Passive] Đã áp dụng StatusEffect {effectId} ({valText(effectValue, _isReduction, _isPercentage)}) vào chỉ số {_targetStat} của {target.name}");
    }

    private string valText(float val, bool isReduction, bool isPercent)
    {
        return $"{(isReduction ? "-" : "+")}{val}{(isPercent ? "%" : "")}";
    }
}
