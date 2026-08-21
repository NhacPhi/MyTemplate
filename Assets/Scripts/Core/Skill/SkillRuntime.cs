using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void ResetCooldown()
    {
        CurrentCooldown = 0;
    }

    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
        }
    }

    public void ReduceCooldown(int amount = 1)
    {
        CurrentCooldown = Math.Max(0, CurrentCooldown - amount);
    }

    public virtual DamageBonus CalculateRawDamage()
    {
        var bonus = new DamageBonus()
        {
            DamageMultiplier = GetSkillData() != null ? GetSkillData().DamageMultiplier : 1f,
            Tags = new HashSet<string>()
        };

        if (GetSkillData() != null)
        {
            bonus.Tags.Add(GetSkillData().SkillType.ToString());
        }

        if (owner != null)
        {
            var entityPassive = owner.GetComponent<EntityPassive>();
            if (entityPassive != null)
            {
                var casterEntity = owner.GetComponent<Entity>();
                var targetEntity = casterEntity != null && casterEntity.Target != null 
                    ? casterEntity.Target.GetComponent<Entity>() 
                    : null;
                entityPassive.ProcessDamageBonus(ref bonus, targetEntity, bonus.Tags);
            }
        }

        return bonus;
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
            if (attachedEffect == null) continue;

            // Kiểm tra khuếch đại hiệu quả Buff từ nội tại vũ khí (ví dụ Cửu Hằng Trượng psv_ninefold_staff)
            float bonusBuffPercent = 0f;
            if (caster != null && (attachedEffect.IsBuff() || GetSkillData().IsBuffSkill))
            {
                var casterPassive = caster.GetComponent<EntityPassive>();
                if (casterPassive != null && casterPassive.ActivePassives != null)
                {
                    foreach (var passive in casterPassive.ActivePassives)
                    {
                        if (passive.Config != null && passive.Config.ID == "psv_ninefold_staff")
                        {
                            bonusBuffPercent += passive.GetCombatEventValue(0);
                            break;
                        }
                    }
                }
            }

            EffectConfig effectToApply = attachedEffect;
            if (bonusBuffPercent > 0f)
            {
                // Công thức khuếch đại: Giá trị mới = Giá trị gốc * (1 + bonusBuffPercent / 100)
                // Ví dụ: 20% ATK buff được tăng 35% hiệu quả -> 20 * (1 + 0.35) = 27% ATK
                float amplifiedValue = attachedEffect.Value * (1f + (bonusBuffPercent / 100f));
                effectToApply = new EffectConfig
                {
                    Name = attachedEffect.Name,
                    Description = attachedEffect.Description,
                    Type = attachedEffect.Type,
                    TargetStat = attachedEffect.TargetStat,
                    ModifyType = attachedEffect.ModifyType,
                    Duration = attachedEffect.Duration,
                    MaxStack = attachedEffect.MaxStack,
                    Value = Mathf.RoundToInt(amplifiedValue)
                };
            }

            StatusEffect newEffect = EffectFactory.CreateEffect(GetSkillData().ID, effectToApply, targetStats);

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

            case SkillTargetType.SameRowAllies:
                if (BattleManager.Instance != null)
                {
                    var allies = BattleManager.Instance.GetEntitiesByTeam(caster.Team);
                    foreach (var ally in allies)
                    {
                        if (ally != null && ally.GetCoreComponent<EntityStats>() != null && !ally.GetCoreComponent<EntityStats>().IsDead && ally.Row == caster.Row)
                        {
                            targetList.Add(ally);
                        }
                    }
                }
                else
                {
                    targetList.Add(caster);
                }
                break;

                // Xử lý các trường hợp đặc biệt khác (Column, Row, DeadAlly...)
        }

        return targetList;
    }

    public virtual void Dispose()
    {
    }
}
