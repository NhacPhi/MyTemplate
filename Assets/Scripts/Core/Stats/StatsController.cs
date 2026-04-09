using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Tech.Composite;
using System;
using UnityEditor.VersionControl;

public class StatsController : CoreComponent, IEffectable 
{
    //protected CharacterProfileModel statsHolder;
    protected IStatProvider _statProvider;

    [field: SerializeField] public string EntityID { get; protected set; }

    protected List<StatusEffect> statusEffects = new List<StatusEffect>();
    public IReadOnlyCollection<StatusEffect> StatusEffect => statusEffects.AsReadOnly();

    protected Dictionary<StatType, Stat> stats;
    public Dictionary<StatType, Stat> Stats => stats;

    public Action<StatEvtArgs> OnStatChange;

    protected Dictionary<AttributeType, Attribute> attributes;
    public Dictionary<AttributeType, Attribute> Attributes => attributes;

    // Update Hearth bar
    public Action<AttributeEvtArgs> OnAttributeChange;

    // Update Effect 
    public Action<StatusEffect> OnEffectAdded;  
    public Action<StatusEffect> OnEffectRemoved; 
    public Action<StatusEffect> OnEffectUpdated;

    private float _currentAV;
    public float CurrentAV
    {
        get => _currentAV;
        set
        {
            _currentAV = Mathf.Max(0, value); ;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Only Work On Editor
    /// </summary>
    public Action NotifyEditor;
#endif

    public override void LoadComponent()
    {
        base.LoadComponent();
        //InitAttribute();
    }

    public void Setup(IStatProvider provider)
    {
        _statProvider = provider;
        InitAttribute();
    }

    private void InitAttribute()
    {
        if(attributes != null)
        {
            return;
        }

        attributes = new Dictionary<AttributeType, Attribute>();


        InitStats();

        foreach (AttributeType key in _statProvider.BaseConfig.Attributes.Keys)
        {
            Stat maxStat = null;
            AttributeComponent attributeConponent = _statProvider.BaseConfig.Attributes[key];
            if(attributeConponent.StatMaxStatType != StatType.None)
            {
                maxStat = stats[attributeConponent.StatMaxStatType];
            }
            Attribute attribute = new(0, maxStat, attributeConponent.StartPercent, this);
#if UNITY_EDITOR
            attribute.OnValueChange += HandleNotifyEditor;
#endif

            attributes.Add(key, attribute);
        }
#if UNITY_EDITOR
        NotifyEditor?.Invoke();
#endif
    }
    public void InitStats()
    {
        if (stats != null) return;

        this.stats = new Dictionary<StatType, Stat>();


        foreach (var key in _statProvider.BaseConfig.Stats.Keys)
        {
            Stat stat = new(_statProvider.GetTotalStat(key));
#if UNITY_EDITOR
            stat.OnValueChange += HandleNotifyEditor;
#endif
            stats.Add(key, stat);
        }
#if UNITY_EDITOR
        NotifyEditor?.Invoke();
#endif
    }

    public Attribute GetAttribute(AttributeType type)
    {
        InitAttribute();
        return attributes.GetValueOrDefault(type);
    }

    public Stat GetStat(StatType type)
    {
        InitStats();
        return stats.GetValueOrDefault(type);
    }

    public void UpdateStat(StatEvtArgs stats)
    {
        OnStatChange?.Invoke(stats);
    }

    public virtual void Renew()
    {
        foreach(Stat stat in stats.Values)
        {
            stat.ClearAllModifiers();
        }

        foreach (var effect in statusEffects)
        {
            effect.Stop();
        }

        statusEffects.Clear();
    }

    public virtual void AddModifier(StatType type, Modifier modifier)
    {
        if (!stats.TryGetValue(type, out Stat value)) return;

        value.AddModifier(modifier);
    }

    public virtual void RemoveModifier(StatType type, Modifier modifier)
    {
        stats[type].RemoveModifier(modifier);
    }

    public virtual void AddModifierWithoutNotify(StatType type, Modifier modifier)
    {
        if (!stats.TryGetValue(type, out Stat value)) return;

        value.AddModifierWithoutNotify(modifier);
    }

    public virtual void RemoveModifierWithoutNotify(StatType type, Modifier modifier)
    {
        stats[type].RemoveModifierWithoutNotify(modifier);
    }

    public void ApplyEffect(StatusEffect effect, int currentTurnID)
    {
        if (effect == null || effect.MaxStack <= 0) return;

        bool isExist = FindEffect(effect, out var existEffect);

        // 1. NẾU CHƯA TỒN TẠI (Hoặc tồn tại nhưng IsUnique = false -> Cho phép tạo mới)
        if (!isExist || !effect.IsUnique)
        {
            var clone = effect.Clone();
            statusEffects.Add(clone);
            clone.StartEffect(currentTurnID);

            OnEffectAdded?.Invoke(clone);
            return;
        }

        // 2. NẾU ĐÃ TỒN TẠI VÀ LÀ HIỆU ỨNG ĐỘC NHẤT (IsUnique = true)
        if (existEffect.IsStackable)
        {
            if (existEffect.CurrentStack < existEffect.MaxStack)
            {
                // Vẫn còn chỗ -> Tăng Stack (Thường trong hàm AddStack bạn cũng đã có lệnh turn = 0 rồi)
                existEffect.AddStack(currentTurnID);
            }
            else
            {
                // Đã Max Stack -> Không tăng độ đau, chỉ làm mới thời gian
                existEffect.ResetDuration();
            }

            OnEffectUpdated?.Invoke(existEffect);
        }
        else
        {
            // Không cho cộng dồn (VD: Choáng) -> Chỉ làm mới thời gian
            existEffect.ResetDuration();
        }
    }
    public void RemoveEffect(StatusEffect effect, bool ignoreStack)
    {
        if(!FindEffect(effect, out var cloneEffect)) return;
        
        if(ignoreStack)
        {
            cloneEffect.Stop();
            statusEffects.Remove(cloneEffect);
            OnEffectRemoved?.Invoke(cloneEffect);
            return;
        }

        if (effect.IsStackable && cloneEffect.RemoveStack() > 0)
        {
            OnEffectUpdated?.Invoke(cloneEffect);
            return;
        }

        cloneEffect.Stop();
        statusEffects.Remove(cloneEffect);

        OnEffectRemoved?.Invoke(cloneEffect);
    }
    public bool HasEffect<T>() where T : StatusEffect
    {
        foreach (var effect in statusEffects)
        {
            if (effect.GetType() == typeof(T)) return true;
        }

        return false;
    }

    protected bool FindEffect(StatusEffect effect, out StatusEffect cloneEffect)
    {
        cloneEffect = null;
        if (effect == null) return false;


        foreach (var m_effect in statusEffects)
        {
            if (!effect.Equals(m_effect)) continue;

            cloneEffect = m_effect;
            return true;
        }

        return false;
    }

    public void ProcessStartOfTurn()
    {
        if (statusEffects == null || statusEffects.Count == 0) return;

        // Cho các effect chạy logic đầu lượt (VD: Giải trừ, Hồi Năng lượng)
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].OnStartOfTurn();
        }
    }

    public void ProcessEndOfTurn(int currentTurnID)
    {
        if (statusEffects == null || statusEffects.Count == 0) return;

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];

            // 1. Chạy logic cuối lượt (VD: Trừ máu độc, Hồi máu Regen)
            effect.OnEndOfTurn();

            // 2. Giảm thời gian tồn tại của Effect
            effect.ReduceDuration(currentTurnID);

            // 3. Dọn dẹp nếu hết hạn
            if (effect.IsStop)
            {
                statusEffects.RemoveAt(i);
                OnEffectRemoved?.Invoke(effect); // Báo UI tắt Icon
            }
            else
            {
                OnEffectUpdated?.Invoke(effect); // Báo UI cập nhật số Hiệp
            }
        }
    }

    public bool CanTakeTurn()
    {
        foreach (var effect in statusEffects)
        {
            // Tùy vào cách bạn định nghĩa Enum EffectType trong EffectConfig
            // Ví dụ: if (effect.Data.EffectType == EffectType.Stun) return false;

            if (effect.Data.Type == EffectType.Stun) return false;
        }
        return true;
    }


    public void CalculateStatsValue()
    {
        foreach(var stat in Stats)
        {
            stat.Value.ReCalculateValue();
        }
    }

#if UNITY_EDITOR
    private void HandleNotifyEditor(object value)
    {
        NotifyEditor?.Invoke();
    }
#endif
}
