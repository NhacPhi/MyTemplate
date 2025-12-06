using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class TurnEndState : BattleBaseState
{
    public TurnEndState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Handle effect (buff or debuff)
        // Reset param vd: HasActionComplete
        // check win/lose condition
        // win => WinState, lose => LoseState
        // continue check next Unit next turn and set next State by UnitType
    }
}
