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

    public override void Execute(Entity caster)
    {
        //Trigger Animation
        EntityStateData state = caster.GetComponent<EntityStateData>();
        if (state != null)
        {
            state.NextStateAfterMoveext = EntityState.MAIN_SKILL;
            //state.StateManager.ChangeState(EntityState.MOVE_UP);
        }
    }
}

public class EmpoweredAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new EmpoweredAttack(owner, this);
}

