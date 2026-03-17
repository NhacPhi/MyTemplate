using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoveDown : EntityStateBase
{
    private Vector2 _currentVelocity = Vector2.zero;

    private bool _isSortingRender;

    private float _dist = 0;
    public EntityMoveDown(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
        _isSortingRender = false;
        _dist = Vector3.Distance(data.RootPosition, data.Entity.Target.transform.position);
    }

    public override void LogicUpdate()
    {
        if (!_isSortingRender)
        {
            var dist = Vector3.Distance(data.Entity.transform.position, data.RootPosition);
            if (dist < _dist / 2)
            {
                data.Entity.SetRenderOrder(data.Entity.RenderOrder);
                _isSortingRender = true;
            }
        }

        if (!MoveToRoot()) return;

        data.TriggerMoveEnd();

        data.StateManager.ChangeState(EntityState.IDLE);
    }

    protected virtual bool MoveToRoot()
    {
        var entityTransform = data.Entity.transform;
        var targetPos = data.RootPosition;

        //entityTransform.position = Vector2.MoveTowards(entityTransform.position,
        //    targetPos, data.MoveSpeed * Time.deltaTime);

        entityTransform.position = Vector2.SmoothDamp(
            entityTransform.position,
            targetPos,
            ref _currentVelocity,
            data.SmoothTime,
            data.MoveSpeed
        );

        return Vector3.Distance(entityTransform.position, targetPos) < 0.01f;
    }
}
