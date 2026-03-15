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

    public override void Execute(Entity caster)
    {
        var enemy_ultimate = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy_ultimate, true);
        //Trigger Animation
        EntityStateData state = caster.GetComponent<EntityStateData>();
        if (state != null)
        {
            state.NextStateAfterMoveNext = EntityState.MAIN_SKILL;
        }
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {

    }
}

public class EmpoweredAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new EmpoweredAttack(owner, this);
}

