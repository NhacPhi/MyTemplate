using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoveUp : EntityStateBase
{
    private Vector2 _currentVelocity = Vector2.zero;
    public EntityMoveUp(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
 
    }

    public override void LogicUpdate()
    {
        if (!MoveToTarget()) return;

        data.StateManager.ChangeState(data.NextStateAfterMoveNext);

        data.NextStateAfterMoveNext = EntityState.ATTACK;
    }

    protected virtual bool MoveToTarget()
    {
        var entityTransform = data.Entity.transform;
        var targetPos = data.MovePosition;

        entityTransform.position = Vector2.SmoothDamp(
            entityTransform.position,
            targetPos,
            ref _currentVelocity,
            data.SmoothTime,
            data.MoveSpeed
            );

        return Vector3.Distance(entityTransform.position, targetPos) < 0.2f;
    }
}
