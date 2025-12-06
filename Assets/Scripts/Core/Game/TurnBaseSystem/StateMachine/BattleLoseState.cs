using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class BattleLoseState : BattleBaseState
{
    public BattleLoseState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Show lose UI 
        // Shop option: Retry or return main menu
    }
}
