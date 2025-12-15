using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class EntityStateBase : BaseState
{
    protected EntityStateData data;

    public EntityStateBase(EntityStateData data)
    {
        this.data = data;
    }

    public override void Enter() { }
    public override void Exit() { }
    public virtual void LogicUpdate() { }
}
