using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tech.Composite;
using Cysharp.Threading.Tasks;

public class EnemyDecision
{
    public SkillCharacter SkillType;
    public Entity Target;
}

public abstract class EnemyBrain : CoreComponent
{
    protected Entity _entity;

    public override void LoadComponent()
    {
        _entity = core as Entity;
    }

    protected Entity GetLowestHpTarget(List<Entity> aliveTargets)
    {
        if (aliveTargets == null || aliveTargets.Count == 0) return null;

        var valid = aliveTargets
             .Where(p => p != null && p.GetCoreComponent<EntityStats>() != null && !p.GetCoreComponent<EntityStats>().IsDead);

        return valid.OrderBy(p => p.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value).FirstOrDefault();
    }

    public abstract UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam);
}
