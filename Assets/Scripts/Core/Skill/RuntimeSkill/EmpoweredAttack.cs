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

    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();

        caster.HandleTurn(enemy);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MOVE_UP);

        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.MAIN_SKILL);

        caster.PlaySFX(skillData.Sound);

        await state.WaitForHitFrame();

        if (!enemy.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(caster, currentTurnID);
        }

        DamageFormular.DealDamage(CalculateRawDamage(), caster, enemy);

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

