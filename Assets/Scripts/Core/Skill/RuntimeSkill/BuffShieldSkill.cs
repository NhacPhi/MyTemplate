using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class BuffShieldSkill : SkillRuntime
{
    private BuffShieldData skillData;

    public BuffShieldSkill(EntityStats owner, BuffShieldData skillData) : base(owner)
    {
        this.skillData = skillData;
    }
    public override async UniTask ExecuteAsync(Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);

        await state.WaitForHitFrame();

        EntityStats stat = caster.GetCoreComponent<EntityStats>();
        stat.BuffShield(1000);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

}

public class BuffShieldData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new BuffShieldSkill(owner, this);
}

