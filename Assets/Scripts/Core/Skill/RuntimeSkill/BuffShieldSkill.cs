using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffShieldSkill : SkillRuntime
{
    private BuffShieldData skillData;

    public BuffShieldSkill(EntityStats owner, BuffShieldData skillData) : base(owner)
    {
        this.skillData = skillData;
    }
    public override void Execute(Entity caster)
    {
        //MajorSkill
        //Trigger Animation
        EntityStateData stateData = caster.GetComponent<EntityStateData>();
        EntityStats state = caster.GetComponent<EntityStats>();
        if (state != null)
        {
            stateData.StateManager.ChangeState(EntityState.MAJOR_SKILL);
            state.BuffShield(1000);
            //state.StateManager.ChangeState(EntityState.MOVE_UP);
        }
    }

    public override SkillData GetSkillData() => skillData;

}

public class BuffShieldData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new BuffShieldSkill(owner, this);
}

