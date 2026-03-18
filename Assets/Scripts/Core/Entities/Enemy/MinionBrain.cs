using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionBrain : EnemyBrain
{
    public override async UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam)
    {
        var aliveTargets = playerTeam.Where(p => !p.GetCoreComponent<EntityStats>().IsDead).ToList();

        Entity randomTarget = GetLowestHpTarget(playerTeam);

        return new EnemyDecision
        {
            SkillType = SkillCharacter.Base,
            Target = randomTarget
        };
    }
}
