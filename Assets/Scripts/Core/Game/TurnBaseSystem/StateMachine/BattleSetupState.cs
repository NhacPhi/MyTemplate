using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class BattleSetupState : BattleBaseState
{
    public BattleSetupState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Init UI
        // Spawn Units()
        // Setting Turn Order
        // Swtich State
    }

    public override void Exit() 
    {

    }
}
