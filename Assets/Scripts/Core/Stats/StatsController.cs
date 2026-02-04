using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Tech.Composite;
using System;

public class StatsController : CoreComponent, IEffectable 
{
    //protected StatsDataHolder statsHolder;
    protected CharacterConfig configHolder;
    [Inject] GameDataBase DataBase;

    [field: SerializeField] public string EntityID { get; protected set; }

    //protected List<StatusEffect> statusEffects = new();
    //public IReadOnlyCollection<StatusEffect> StatusEffect => statusEffects.AsReadOnly();

    protected Dictionary<StatType, Stat> stats;
    public Dictionary<StatType, Stat> Stats => stats;

    public Action<StatEvtArgs> OnStatChange;

    protected Dictionary<AttributeType, Attribute> attributes;
    public Dictionary<AttributeType, Attribute> Atributes => attributes;

    public Action<AttributeEvtArgs> OnAtributeChange;

#if UNITY_EDITOR
    /// <summary>
    /// Only Work On Editor
    /// </summary>
    public Action NotifyEditor;
#endif

    public override void LoadComponent()
    {
        base.LoadComponent();
        InitAttribute();
    }

    private void InitAttribute()
    {
        if(attributes != null)
        {
            return;
        }

        attributes = new Dictionary<AttributeType, Attribute>();

        if(configHolder == null)
        {
            configHolder = DataBase.GetCharacterConfig(EntityID);
        }

        InitStats();

        foreach(AttributeType key in configHolder.Attributes.Keys)
        {
            Stat maxStat = null;
            AttributeComponent attributeConponent = configHolder.Attributes[key];
            if(attributeConponent.StatMaxStatType != StatType.None)
            {
                maxStat = stats[attributeConponent.StatMaxStatType];
            }
            Attribute attribute = new(0, maxStat, attributeConponent.SttartPercent, this);
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

        if (configHolder == null)
        {
            Debug.Log("Object: " + gameObject.name);
            configHolder = DataBase.GetCharacterConfig(EntityID);
        }


        this.stats = new Dictionary<StatType, Stat>();

        foreach (var key in configHolder.Stats.Keys)
        {
            Stat stat = new(configHolder.Stats[key]);
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

        //foreach(var effect in statusEffects)
        //{
        //    effect.Stop();
        //}

        //statusEffects.Clear();
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
            //statusEffects.Add(clone);
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
            //statusEffects.Remove(cloneEffect);
            return;
        }

        if (effect.IsStackable && cloneEffect.RemoveStack() > 0) return;

        cloneEffect.Stop();
        //statusEffects.Remove(cloneEffect);
    }
    public bool HasEffect<T>() where T : StatusEffect
    {
        //foreach(var effect in statusEffects)
        //{
        //    if(effect.GetType() == typeof(T)) return true;
        //}

        return false;
    }

    protected bool FindEffect(StatusEffect effect, out StatusEffect cloneEffect)
    {
        cloneEffect = null;
        if(effect == null) return false;

        //foreach (var m_effect in statusEffects)
        //{
        //    if(!effect.Equals(m_effect)) continue;

        //    cloneEffect = m_effect;
        //    return true;
        //}

        return false;
    }

    protected virtual void Update()
    {
        //if(statusEffects == null) return;
        //for(int i = statusEffects.Count - 1; i >= 0; i--)
        //{
        //    var effect = statusEffects[i];
        //    effect.Update();
        //    if (!effect.IsStop) continue;
        //    statusEffects.RemoveAt(i);
        //}
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
