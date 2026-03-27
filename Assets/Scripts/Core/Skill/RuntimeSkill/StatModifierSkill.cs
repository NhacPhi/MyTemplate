using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifierSkill : SkillRuntime
{
    private StatModifierData skillData;

    public StatModifierSkill(EntityStats owner, StatModifierData skillData) : base(owner)
    {
        this.skillData = skillData;
    }
    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var enemy = caster.Target.gameObject.GetComponent<Entity>();
        caster.HandleTurn(enemy);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(caster.GetCoreComponent<EntitySkill>().MatchSkillCharacterToEntityState(this));
        caster.PlaySFX(skillData.Sound);
        if (!enemy.GetCoreComponent<EntityStats>().IsDead)
        {
            ApplyEffectsToTarget(caster, currentTurnID);
        }

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;
}

public class StatModifierData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new StatModifierSkill(owner, this);
}
