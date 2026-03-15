using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : SkillRuntime, IAttackSkill
{
    private MeleeAttackData skillData;

    public MeleeAttack(EntityStats owner, MeleeAttackData skillData) : base(owner)
    {
        this.skillData = skillData;
    }

    public override SkillData GetSkillData() => skillData;

    public override void Execute(Entity caster)
    {
        var enemy_ultimate = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy_ultimate, false);
    }
    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }
    //public async UniTask Perform(SkillData config, Entity caster)
    //{

    //}
}
public class MeleeAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MeleeAttack(owner, this);
}
