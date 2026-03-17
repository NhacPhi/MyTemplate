using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoveUp : EntityStateBase
{
    private Vector2 _currentVelocity = Vector2.zero;

    private bool _isSortingRender;

    private float _dist = 0;
    public EntityMoveUp(EntityStateData data) : base(data)
    {

    }

    public override void Enter()
    {
        _isSortingRender = false;
        _dist = Vector3.Distance(data.Entity.transform.position, data.Entity.Target.transform.position);
    }

    public override void LogicUpdate()
    {
        if(!_isSortingRender)
        {
            var dist = Vector3.Distance(data.Entity.transform.position, data.Entity.Target.transform.position);
            if(dist < _dist/2)
            {
                data.Entity.SetRenderOrder(data.Entity.Target.gameObject.GetComponent<Entity>().RenderOrder + 1);
                _isSortingRender = true;
            }
        }

        if (!MoveToTarget()) return;

        data.TriggerMoveEnd();
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
