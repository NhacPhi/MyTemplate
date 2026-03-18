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
        return aliveTargets
             .Where(p => !p.GetCoreComponent<EntityStats>().IsDead)
             .OrderBy(p => p.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value)
             .First();
    }

    public abstract UniTask<EnemyDecision> DecideAsync(List<Entity> playerTeam);
}
