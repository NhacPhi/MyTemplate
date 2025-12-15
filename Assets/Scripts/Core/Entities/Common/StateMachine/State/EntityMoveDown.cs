using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoveDown : EntityStateBase
{
    public EntityMoveDown(EntityStateData data) : base(data)
    {

    }

    public override void LogicUpdate()
    {
        if (!MoveToRoot()) return;

        data.StateManager.ChangeState(EntityState.IDLE);
    }

    protected virtual bool MoveToRoot()
    {
        var entityTransform = data.Entity.transform;
        var targetPos = data.RootPosition;

        entityTransform.position = Vector2.MoveTowards(entityTransform.position,
            targetPos, data.MoveSpeed * Time.deltaTime);

        return Vector3.Distance(entityTransform.position, targetPos) < 0.01f;
    }
}
