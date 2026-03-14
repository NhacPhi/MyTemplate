using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class BeginTurnBase : BattleBaseState
{
    // Effect Xử lý sát thương / Hồi phục theo thời gian
    //Kiểm tra hiệu ứng khống chế (Crowd Control)
    //Kích hoạt Kỹ năng Bị động (Passive Skills)
    public BeginTurnBase(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }
}
