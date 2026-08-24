using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpoweredAttack : SkillRuntime, IAttackSkill
{
    private EmpoweredAttackData skillData;
    public EmpoweredAttack(EntityStats owner, EmpoweredAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override DamageBonus CalculateRawDamage()
    {
        return base.CalculateRawDamage();
    }

    public void OnDealDamage(ref float damageInput)
    {
        // Handled dynamically by Event Handlers in Damage Pipeline
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var enemy = GetValidSingleTarget(caster);
        if (enemy == null)
        {
            PutOnCooldown();
            return;
        }

        caster.HandleTurn(enemy);

        // Thu thập danh sách mục tiêu (Hỗ trợ Đơn Mục Tiêu, Toàn Hàng, Toàn Bộ Kẻ Địch)
        List<Entity> targetEnemies = new List<Entity>();
        if (skillData.TargetType == SkillTargetType.AllEnemies || skillData.TargetType == SkillTargetType.EnemyRow)
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
            ApplyEffectsToTarget(caster, currentTurnID);
            foreach (var target in targetEnemies)
            {
                if (target != null && target.GetCoreComponent<EntityStats>() != null && !target.GetCoreComponent<EntityStats>().IsDead)
                {
                    DamageFormular.DealDamage(CalculateRawDamage(), caster, target);
                }
            }
            PutOnCooldown();
            return;
        }

        caster.StateManager.ChangeState(EntityState.MOVE_UP);

        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);

        caster.PlaySFX(skillData.Sound);

        await state.WaitForHitFrame();

        ApplyEffectsToTarget(caster, currentTurnID);

        foreach (var target in targetEnemies)
        {
            if (target != null && target.GetCoreComponent<EntityStats>() != null && !target.GetCoreComponent<EntityStats>().IsDead)
            {
                DamageFormular.DealDamage(CalculateRawDamage(), caster, target);
            }
        }

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.MOVE_DOWN);

        await state.WaitForMoveEnd();

        PutOnCooldown();
    }
}

public class EmpoweredAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new EmpoweredAttack(owner, this);
}

