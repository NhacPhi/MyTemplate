using System;
using UnityEngine;

public interface IDamagable 
{
    public bool IsDead { get; }
    public Action OnDeath { get; set; }
    public Action<float, Transform> OnHit { get; set; }
    public void TakeDamage(float damage, Transform attacker);
    public Attribute GetAttribute(AttributeType type);
    public Stat GetStat(StatType type);
}
