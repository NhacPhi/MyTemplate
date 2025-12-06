using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class PlayerTurnState : BattleBaseState
{
    public PlayerTurnState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Get character
        // Show ability UI of character
        // Turn on watting input flag setup = true
    }

    public override void Exit()
    {
        // hide ability UI of character
        // Turn on watting input flag setup = false
    }

    public override void OnUpdate()
    {
        // Check action complete
        // Swith Endturn
    }
}
