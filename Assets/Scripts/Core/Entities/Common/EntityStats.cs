using UnityEditor;
using UnityEngine;
using VContainer;
using System;

public class EntityStats : StatsController, IDamagable
{
    public bool IsDead { get; protected set; }
    public Action OnDeath { get; set; }
    public Action<float, Transform> OnHit { get; set; }
    public EntitySkill Skill { get; protected set; }

    protected virtual void Start()
    {
        Skill = this.core.GetCoreComponent<EntitySkill>();
    }

    public override void LoadComponent()
    {
        base.LoadComponent();
        //CallEvent();
    }

    public virtual void TakeDamage(float damage, Transform attacker)
    {
        OnHit?.Invoke(damage, attacker);
        var hp = GetAttribute(AttributeType.Hp);
        hp.Value -= damage;

        OnStatChange?.Invoke(new StatEvtArgs()
        {
            Stat = StatType.HP,
            Value = damage,
        });

        if (!(hp.Value <= 0)) return;

        HandleDeath();
    }

    protected virtual void HandleDeath()
    {

    }
    public override void Renew()
    {

    }

    private void CallEvent()
    {
        //Test
        //var hp = GetStat(StatType.HP);


        //OnStatChange?.Invoke(new StatEvtArgs()
        //{
        //    Stat = StatType.HP,
        //    Value = hp.Value,
        //});


    //    def.OnValueChange += (args) =>
    //    {
    //        PlayerStatusAction.OnDefChange?.Invoke(args);
    //    };

    //    atk.OnValueChange += (args) =>
    //    {
    //        PlayerStatusAction.OnAtkChange?.Invoke(args);
    //    };
    }
}