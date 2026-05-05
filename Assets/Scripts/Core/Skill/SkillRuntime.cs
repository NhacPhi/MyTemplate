using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

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

    public abstract UniTask ExecuteAsync(Entity caster,int currentTurnID = 0);

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

    protected virtual void ApplyEffectsToTarget(Entity caster, int currentTurnID)
    {
        var targetEnities = GetEffectTargets(caster);

        if (targetEnities == null) return;

        foreach ( var target in targetEnities )
        {
            var targetStats = target.GetCoreComponent<StatsController>();


            if (targetStats == null) continue;

            var attachedEffect = GetSkillData().Effect;

            StatusEffect newEffect = EffectFactory.CreateEffect(GetSkillData().ID, attachedEffect, targetStats);

            targetStats.ApplyEffect(newEffect, currentTurnID);
        }

       
    }

    private List<Entity> GetEffectTargets(Entity caster)
    {
        List<Entity> targetList = new List<Entity>();

        switch (GetSkillData().TargetType)
        {
            case SkillTargetType.Self:
                targetList.Add(caster);
                break;

            case SkillTargetType.SingleEnemy:
                if (caster.Target != null) targetList.Add(caster.Target.gameObject.GetComponent<Entity>());
                break;

            case SkillTargetType.SingleAlly:
                // Giả sử entitySelect lúc này đóng vai trò là đồng minh được chọn
                if (caster.Target != null) targetList.Add(caster.Target.gameObject.GetComponent<Entity>());
                break;

            case SkillTargetType.AllEnemies:
                if (caster.Targets != null)
                    foreach (var target in caster.Targets) { targetList.Add(target.gameObject.GetComponent<Entity>()); }
                break;

            case SkillTargetType.AllAllies:
                if (caster.Targets != null)
                    foreach (var target in caster.Targets) { targetList.Add(target.gameObject.GetComponent<Entity>()); }
                break;

                // Xử lý các trường hợp đặc biệt khác (Column, Row, DeadAlly...)
        }

        return targetList;
    }
}
