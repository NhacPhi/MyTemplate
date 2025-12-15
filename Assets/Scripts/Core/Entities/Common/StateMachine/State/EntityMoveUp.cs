using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoveUp : EntityStateBase
{
    public EntityMoveUp(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
 
    }

    public override void LogicUpdate()
    {
        if (!MoveToTarget()) return;

        data.StateManager.ChangeState(EntityState.ATTACK);
    }

    protected virtual bool MoveToTarget()
    {
        var entityTransform = data.Entity.transform;
        var targetPos = data.MovePosition;

        entityTransform.position = Vector2.MoveTowards(entityTransform.position,
            targetPos, data.MoveSpeed * Time.deltaTime);

        return Vector3.Distance(entityTransform.position, targetPos) < 0.01f;
    }
}
