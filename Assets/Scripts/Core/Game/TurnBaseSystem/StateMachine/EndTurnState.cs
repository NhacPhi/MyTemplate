using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class EndTurnState : BattleBaseState
{
    // Trừ thời gian của Buff / Debuff (Duration Tick)
    //Reset các chỉ số tạm thời
    public EndTurnState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        battleManager.TurnSystem.ResetEntityAV(battleManager._CurrentCharacter);
        battleManager.StateMachine.ChangeState(BattleState.OrderState);
    }

    public override void Exit()
    {

    }
}
