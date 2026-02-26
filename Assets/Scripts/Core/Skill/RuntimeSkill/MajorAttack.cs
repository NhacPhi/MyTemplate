using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MajorAttack : SkillRuntime, IAttackSkill
{
    private MajorAttackData skillData;

    public MajorAttack(EntityStats owner, MajorAttackData skillData) : base(owner)
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
        _ = PerformSummon(skillData, caster);
    }

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        //Trigger Animation
        EntityStateData state = caster.GetComponent<EntityStateData>();
        if (state != null)
        {
            state.NextStateAfterMoveNext = EntityState.MAJOR_SKILL;
            state.StateManager.ChangeState(EntityState.MOVE_UP);
        }
        await UniTask.Delay(2000);
        var damage = new DamageBonus()
        {
            FlatValue = 0,
            DamageMultiplier = 1.5f
        };

        DamageFormular.DealDamage(damage, caster, caster.target.GetComponent<Entity>());
    }
}

public class MajorAttackData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new MajorAttack(owner, this);
}

