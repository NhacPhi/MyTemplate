using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MajorAttack : SkillRuntime, IAttackSkill
{
    private MajorAttackData skillData;

    public MajorAttack(EntityStats owner, MajorAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override DamageBonus CalculateRawDamage()
    {
        var bonus = base.CalculateRawDamage();
        if (bonus.Tags == null) bonus.Tags = new HashSet<string>();
        bonus.Tags.Add("MajorSkill");
        return bonus;
    }

    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        await PerformSkillAsync(skillData, caster, currentTurnID);
    }

    public async UniTask PerformSkillAsync(SkillData config, Entity caster, int currentTurnID)
    {
        var enemy = GetValidSingleTarget(caster);
        if (enemy == null)
        {
            PutOnCooldown();
            return;
        }

        caster.HandleTurn(enemy);

        // Thu thập danh sách mục tiêu (Hỗ trợ cả Đơn Mục Tiêu, Toàn Đội và Toàn Hàng)
        List<Entity> targetEnemies = new List<Entity>();
        if (skillData.TargetType == SkillTargetType.EnemyRow || skillData.TargetType == SkillTargetType.AllEnemies)
        {
            if (BattleManager.Instance != null && BattleManager.Instance.TargetSystem != null)
            {
                targetEnemies = BattleManager.Instance.TargetSystem.GetTargets(caster, skillData.TargetType, enemy, BattleManager.Instance.ActiveEntities);
            }
            if (targetEnemies == null || targetEnemies.Count == 0)
            {
                targetEnemies = new List<Entity>() { enemy };
            }
        }
        else
        {
            targetEnemies = new List<Entity>() { enemy };
        }

        var state = caster.GetCoreComponent<EntityStateData>();
        if (state == null)
        {
            foreach (var target in targetEnemies)
            {
                if (target != null && target.GetCoreComponent<EntityStats>() != null && !target.GetCoreComponent<EntityStats>().IsDead)
                {
                    DamageFormular.DealDamage(CalculateRawDamage(), caster, target);
                }
            }
            ApplyEffectsToTarget(caster, currentTurnID);
            PutOnCooldown();
            return;
        }

        caster.StateManager.ChangeState(EntityState.MOVE_UP);

        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);
        caster.PlaySFX(skillData.Sound);
        await state.WaitForHitFrame();

        foreach (var target in targetEnemies)
        {
            if (target != null && target.GetCoreComponent<EntityStats>() != null && !target.GetCoreComponent<EntityStats>().IsDead)
            {
                DamageFormular.DealDamage(CalculateRawDamage(), caster, target);
            }
        }

        ApplyEffectsToTarget(caster, currentTurnID);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.MOVE_DOWN);

        await state.WaitForMoveEnd();

        PutOnCooldown();
    }
}

public class MajorAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MajorAttack(owner, this);
}

