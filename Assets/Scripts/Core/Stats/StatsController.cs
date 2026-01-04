using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Tech.Composite;
using System;

public class StatsController : CoreComponent, IEffectable 
{
    [field: SerializeField] public string EntityID { get; protected set; }
    protected Dictionary<StatType, Stat> stats;
    protected List<StatusEffect> statusEffects = new();
    public Dictionary<StatType, Stat> Stats => stats;
    internal IReadOnlyCollection<StatusEffect> StatusEffect => statusEffects.AsReadOnly();

#if UNITY_EDITOR
    /// <summary>
    /// Only Work On Editor
    /// </summary>
    public Action NotifyEditor;
#endif

    public void InitStats(Dictionary<StatType, Stat> stats)
    {
        if (stats != null) return;

        this.stats = new Dictionary<StatType, Stat>();

        foreach (var kv in stats)
        {
            this.stats.Add(kv.Key, kv.Value);
        }
        // stats.add(stat)
    }

    public virtual void Renew()
    {
        foreach(Stat stat in stats.Values)
        {
            stat.ClearAllModifiers();
        }

        foreach(var effect in statusEffects)
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

    public void ApplyEffect(StatusEffect effect)
    {
        if(effect == null || effect.MaxStack <= 0) return;

        bool isExist = FindEffect(effect, out var existEffect);

        bool stackIsFull = false; 

        if(isExist && effect.IsStackable)
        {
            stackIsFull = existEffect.CurrentStack >= existEffect.MaxStack;
        }

        if (effect.IsUnique && isExist)
        {
            if(existEffect.IsStackable && !stackIsFull)
            {
                existEffect.AddStack();
            }
        }

        if(!isExist)
        {
            var clone = effect.Clone();
            statusEffects.Add(clone);
            clone.StartEffect();
            return;
        }

        if (stackIsFull) return;

        existEffect.AddStack();
    }
    public void RemoveEffect(StatusEffect effect, bool ignoreStack)
    {
        if(!FindEffect(effect, out var cloneEffect)) return;
        
        if(ignoreStack)
        {
            cloneEffect.Stop();
            statusEffects.Remove(cloneEffect);
            return;
        }

        if (effect.IsStackable && cloneEffect.RemoveStack() > 0) return;

        cloneEffect.Stop();
        statusEffects.Remove(cloneEffect);
    }
    public bool HasEffect<T>() where T : StatusEffect
    {
        foreach(var effect in statusEffects)
        {
            if(effect.GetType() == typeof(T)) return true;
        }

        return false;
    }

    protected bool FindEffect(StatusEffect effect, out StatusEffect cloneEffect)
    {
        cloneEffect = null;
        if(effect == null) return false;

        foreach (var m_effect in statusEffects)
        {
            if(!effect.Equals(m_effect)) continue;

            cloneEffect = m_effect;
            return true;
        }

        return false;
    }

    protected virtual void Update()
    {
        if(statusEffects == null) return;
        for(int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];
            effect.Update();
            if (!effect.IsStop) continue;
            statusEffects.RemoveAt(i);
        }
    }

    public Stat GetStat(StatType type)
    {
        return stats.GetValueOrDefault(type);
    }

    public void CalculateStatsValue()
    {
        foreach(var stat in Stats)
        {
            stat.Value.ReCalculateValue();
        }
    }
}
