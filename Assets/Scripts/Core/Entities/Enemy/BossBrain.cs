using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BossBrain : EnemyBrain
{
    public override async UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam)
    {
        Entity weakestTarget = GetLowestHpTarget(playerTeam);

        var stats = weakestTarget.GetCoreComponent<EntityStats>();

        var skillManager = _entity.GetCoreComponent<EntitySkill>();

        if (stats.GetAttribute(AttributeType.Hp).GetPercent() > 0.3f && skillManager.IsSkillReady(SkillCharacter.Ultimate))
        {
            return new EnemyDecision { SkillType = SkillCharacter.Ultimate, Target = weakestTarget };
        }
        else if (skillManager.IsSkillReady(SkillCharacter.Major))
        {
            return new EnemyDecision { SkillType = SkillCharacter.Major, Target = weakestTarget };
        }
        else
        {
            return new EnemyDecision { SkillType = SkillCharacter.Base, Target = weakestTarget };
        }
    }
}
