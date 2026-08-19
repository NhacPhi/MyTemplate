using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class BuffShieldSkill : SkillRuntime
{
    private BuffShieldData skillData;

    public BuffShieldSkill(EntityStats owner, BuffShieldData skillData) : base(owner)
    {
        this.skillData = skillData;
    }
    public override async UniTask ExecuteAsync(Entity caster, int currentTurnID)
    {
        var targetType = skillData.TargetType;
        var targetEntity = caster.Target != null ? caster.Target.gameObject.GetComponent<Entity>() : null;
        if (targetEntity != null) caster.HandleTurn(targetEntity);

        var state = caster.GetCoreComponent<EntityStateData>();

        caster.StateManager.ChangeState(caster.GetCoreComponent<EntitySkill>().MatchSkillCharacterToEntityState(this));
        caster.PlaySFX(skillData.Sound);
        EntityStats stat = caster.GetCoreComponent<EntityStats>();

        // Tính khiên theo % HP Tối Đa (nếu HP > 0) hoặc theo ATK
        float baseStat = (stat.GetStat(StatType.HP) != null && stat.GetStat(StatType.HP).Value > 0)
            ? stat.GetStat(StatType.HP).Value
            : (stat.GetStat(StatType.ATK) != null ? stat.GetStat(StatType.ATK).Value : 1000f);

        var shieldAmount = CalculateRawDamage().DamageMultiplier * baseStat;

        if (targetType == SkillTargetType.SameRowAllies && BattleManager.Instance != null)
        {
            var allies = BattleManager.Instance.GetEntitiesByTeam(caster.Team);
            foreach (var ally in allies)
            {
                if (ally != null && !ally.GetCoreComponent<EntityStats>().IsDead && ally.Row == caster.Row)
                {
                    ally.GetCoreComponent<EntityStats>()?.BuffShield(shieldAmount);
                }
            }
        }
        else if (targetType == SkillTargetType.AllAllies && BattleManager.Instance != null)
        {
            var allies = BattleManager.Instance.GetEntitiesByTeam(caster.Team);
            foreach (var ally in allies)
            {
                if (ally != null && !ally.GetCoreComponent<EntityStats>().IsDead)
                {
                    ally.GetCoreComponent<EntityStats>()?.BuffShield(shieldAmount);
                }
            }
        }
        else
        {
            stat.BuffShield(shieldAmount);
        }

        await state.WaitForAnimEnd();

        caster.StateManager.ChangeState(EntityState.IDLE);

        PutOnCooldown();
    }

    public override SkillData GetSkillData() => skillData;

}

public class BuffShieldData : SkillData
{
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new BuffShieldSkill(owner, this);
}

