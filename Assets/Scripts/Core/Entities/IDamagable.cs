using System;
using UnityEngine;

public interface IDamagable 
{
    public bool IsDead { get; }
    public Action OnDeath { get; set; }
    public Action<float, Transform, System.Collections.Generic.HashSet<string>> OnHit { get; set; }
    public void TakeDamage(float damage, Transform attacker, System.Collections.Generic.HashSet<string> tags = null);
    public Attribute GetAttribute(AttributeType type);
    public Stat GetStat(StatType type);
}
