using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.StateMachine;

public class BattleBaseState : BaseState
{
    protected StateMachine<BattleState, BattleBaseState> stateMachine;

    public BattleBaseState(StateMachine<BattleState, BattleBaseState> stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public virtual void OnUpdate() { }
}
