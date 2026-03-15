using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttack : SkillRuntime, IAttackSkill
{
    private RangeAttackData skillData;

    public RangeAttack(EntityStats owner, RangeAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override void Execute(Entity caster)
    {
        var enemy_ultimate = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy_ultimate, false);

        //Trigger Animation
        EntityStateData state = caster.GetComponent<EntityStateData>();
        if (state != null)
        {
            state.NextStateAfterMoveNext = EntityState.ATTACK;

        }
    }
    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }
    //public async UniTask Perform(SkillData config, Entity caster)
    //{

    //}
}

public class RangeAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new RangeAttack(owner, this);
}
