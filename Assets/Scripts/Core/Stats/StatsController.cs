using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Tech.Composite;
public class StatsController : Component, IEffectable 
{
    [field: SerializeField] public string EntityID { get; protected set; }
    protected Dictionary<StatType, Stat> stats;
    protected List<StatusEffect> statusEffects = new();
    internal Dictionary<StatType, Stat> Stats => stats;
    internal IReadOnlyCollection<StatusEffect> StatusEffect => statusEffects.AsReadOnly();

    private void InitStats()
    {
        if (stats != null) return;

        stats = new Dictionary<StatType, Stat>();


    }
    public void ApplyEffect(StatusEffect effect)
    {

    }
    public void RemoveEffect(StatusEffect effect, bool ignoreStack)
    {

    }
    public bool HasEffect<T>() where T : StatusEffect
    {
        return false;
    }
}
