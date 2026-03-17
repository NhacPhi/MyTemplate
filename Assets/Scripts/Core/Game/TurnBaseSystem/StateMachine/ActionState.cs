using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class ActionState : BattleBaseState
{
    // If player: Bật UI quản lý Battle Scene lên. else enemy tắt CharacterUI chuyến sang ExecutetionState
    // player chọn skill và Target => chuyển sang thực thi với current skill và current Target (xử lý cho kill AOE )
    public ActionState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        battleManager.IsExecutedAction = false;
    }

    public override void OnUpdate()
    {
        if(battleManager.IsExecutedAction)
        {
            battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
        }
    }
}
