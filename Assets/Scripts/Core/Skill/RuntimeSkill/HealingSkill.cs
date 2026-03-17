using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingSkill : SkillRuntime
{
    private HealingData skillData;

    public HealingSkill(EntityStats owner, HealingData skillData) : base(owner)
    {
        this.skillData = skillData;
    }



    public override async UniTask ExecuteAsync(Entity caster)
    {
        _ = PerformSummon(skillData, caster);
    }
    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);

        var state = caster.GetComponent<EntityStateData>();

        caster.StateManager.ChangeState(EntityState.MAJOR_SKILL);

        await state.WaitForHitFrame();

        EntityStats stat = caster.GetComponent<EntityStats>();

        stat.HealingHP(1000);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);
    }


    public override SkillData GetSkillData() => skillData;
}

public class HealingData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new HealingSkill(owner, this);
}
