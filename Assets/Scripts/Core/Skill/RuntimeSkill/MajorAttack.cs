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

    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }

    public override async UniTask ExecuteAsync(Entity caster)
    {
        await PerformSkillAsync(skillData, caster);
    }

    public async UniTask PerformSkillAsync(SkillData config, Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MOVE_UP);

        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);

        await state.WaitForHitFrame();

        DamageFormular.DealDamage(CalculateRawDamage(), caster, enemy);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.MOVE_DOWN);

        await state.WaitForMoveEnd();

        PutOnCooldown();

        //await UniTask.Delay(2000);
        //var damage = new DamageBonus()
        //{
        //    FlatValue = 0,
        //    DamageMultiplier = 1.5f
        //};

        //DamageFormular.DealDamage(damage, caster, caster.Target.GetComponent<Entity>());
    }
}

public class MajorAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MajorAttack(owner, this);
}

