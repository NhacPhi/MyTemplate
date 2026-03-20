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
        var shield = GetAttribute(AttributeType.Shield);

        float remainingDamage = damage;


        // --- BƯỚC 1: XỬ LÝ GIÁP ẢO ---
        if (shield.Value > 0)
        {
            if (shield.Value >= remainingDamage)
            {
                // Shield đủ cân toàn bộ damage
                shield.Value -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                // Shield bị vỡ hoàn toàn, còn dư damage
                remainingDamage -= shield.Value;
                shield.Value = 0;
            }

            // Cập nhật UI cho Shield
            OnAtributeChange?.Invoke(new AttributeEvtArgs()
            {
                Attribute = AttributeType.Shield,
                Value = shield.Value,
                MaxValue = shield.MaxValue,
            });
        }


        if (remainingDamage > 0)
        {
            hp.Value -= remainingDamage;

            // Cập nhật UI cho HP
            OnAtributeChange?.Invoke(new AttributeEvtArgs()
            {
                Attribute = AttributeType.Hp,
                Value = hp.Value,
                MaxValue = hp.MaxValue,
            });
        }

        // --- BƯỚC 3: KIỂM TRA ĐIỀU KIỆN CHẾT ---
        if (hp.Value <= 0)
        {
            HandleDeath();
        }
    }

    public void BuffShield(float value)
    {
        var shield = GetAttribute(AttributeType.Shield);
        shield.Value = value;

        OnAtributeChange?.Invoke(new AttributeEvtArgs()
        {
            Attribute = AttributeType.Shield,
            Value = shield.Value,
            MaxValue = shield.MaxValue,
        });

    }

    public void HealingHP(float value)
    {
        var hp = GetAttribute(AttributeType.Hp);
        hp.Value += value;

        OnAtributeChange?.Invoke(new AttributeEvtArgs()
        {
            Attribute = AttributeType.Hp,
            Value = hp.Value,
            MaxValue = hp.MaxValue,
        });
    }

    protected virtual void HandleDeath()
    {
        IsDead = true;
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }
    public override void Renew()
    {

    }

    private void CallEvent()
    {
        //Test
        var hp = GetAttribute(AttributeType.Hp);
        //OnAtributeChange?.Invoke(new AttributeEvtArgs()
        //{
        //    Attribute = AttributeType.Hp,
        //    Value = hp.Value,
        //    MaxValue= hp.MaxValue,
        //});



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