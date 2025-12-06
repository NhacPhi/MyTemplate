using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class BattleWonState : BattleBaseState
{
    public BattleWonState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Show win UI 
        // handle reward
        // Return main scene
    }
}
