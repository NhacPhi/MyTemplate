using Cysharp.Threading.Tasks;
using System;

public abstract class SkillRuntime
{
    protected EntityStats owner;

    public int CurrentCooldown { get; private set; }
    public SkillRuntime(EntityStats owner)
    {
        this.owner = owner;
        this.CurrentCooldown = 0;
    }

    public abstract SkillData GetSkillData();

    public abstract UniTask ExecuteAsync(Entity caster);

    public bool IsReady()
    {
        return CurrentCooldown <= 0;
    }

    public void PutOnCooldown()
    {
        CurrentCooldown = GetSkillData().MaxCoolDown;
    }

    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
        }
    }

    public virtual DamageBonus CalculateRawDamage()
    {
        return new DamageBonus()
        {
            FlatValue = GetSkillData().FlatDamage,
            DamageMultiplier = GetSkillData().DamageMultiplier
        };
    }

    protected virtual void ApplyEffectsToTarget(Entity target)
    {
        var targetStats = target.GetCoreComponent<StatsController>();
        if (targetStats == null) return;

        var attachedEffect = GetSkillData().Effect;

        StatusEffect newEffect = EffectFactory.CreateEffect(GetSkillData().ID, attachedEffect, targetStats);

        targetStats.ApplyEffect(newEffect);
    }
}
