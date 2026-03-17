using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MeleeAttack : SkillRuntime, IAttackSkill
{
    private MeleeAttackData skillData;

    public MeleeAttack(EntityStats owner, MeleeAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override async UniTask ExecuteAsync(Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();

        caster.HandleTurn(enemy);

        var state = caster.GetComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MOVE_UP);

        await state.WaitForMoveEnd();

        caster.StateManager.ChangeState(EntityState.ATTACK);

        await state.WaitForHitFrame();

        DamageFormular.DealDamage(DamageBonus.GetDefault(), caster, enemy);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.MOVE_DOWN);

        await state.WaitForMoveEnd();
    }
    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }
    //public async UniTask PerformSkillAsync(SkillData config, Entity caster)
    //{

    //}
}
public class MeleeAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MeleeAttack(owner, this);
}
