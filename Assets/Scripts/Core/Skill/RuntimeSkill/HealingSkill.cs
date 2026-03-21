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
        await PerformSkill(skillData, caster);
    }
    public async UniTask PerformSkill(SkillData config, Entity caster)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(caster.GetCoreComponent<EntitySkill>().MatchSkillCharacterToEntityState(this));

        await state.WaitForHitFrame();

        EntityStats stat = caster.GetCoreComponent<EntityStats>();

        var hp = CalculateRawDamage().FlatValue + CalculateRawDamage().DamageMultiplier * stat.GetStat(StatType.ATK).Value;

        stat.HealingHP(hp);

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        PutOnCooldown();
    }


    public override SkillData GetSkillData() => skillData;
}

public class HealingData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new HealingSkill(owner, this);
}
