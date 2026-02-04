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



    public override void Execute(Entity caster)
    {
        _ = PerformSummon(skillData, caster);
    }
    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        EntityStateData stateData = caster.GetComponent<EntityStateData>();
        EntityStats state = caster.GetComponent<EntityStats>();
        if (stateData != null)
        {
            stateData.StateManager.ChangeState(EntityState.MAJOR_SKILL);
            //state.StateManager.ChangeState(EntityState.MOVE_UP);
        }

        await UniTask.Delay(1000);

        state.HealingHP(1000);
    }


    public override SkillData GetSkillData() => skillData;
}

public class HealingData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new HealingSkill(owner, this);
}
