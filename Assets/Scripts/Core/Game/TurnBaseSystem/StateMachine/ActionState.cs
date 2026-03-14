using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class ActionState : BattleBaseState
{
    // If player: Bật UI quản lý Battle Scene lên. else enemy tắt CharacterUI chuyến sang ExecutetionState
    // player chọn skill và target => chuyển sang thực thi với current skill và current target (xử lý cho kill AOE )
    public ActionState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }
}
