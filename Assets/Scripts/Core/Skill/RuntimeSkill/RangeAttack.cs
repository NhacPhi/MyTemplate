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

    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var enemy_ultimate = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy_ultimate);

        //Trigger Animation
        EntityStateData state = caster.GetCoreComponent<EntityStateData>();

    }
    public void OnDealDamage(ref float damgeInput)
    {
        // Caculate damge
    }
    //public async UniTask PerformSkillAsync(SkillData config, Entity caster)
    //{

    //}
}

public class RangeAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new RangeAttack(owner, this);
}
