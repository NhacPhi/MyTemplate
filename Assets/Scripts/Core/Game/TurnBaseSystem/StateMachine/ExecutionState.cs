using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class ExecutionState : BattleBaseState
{
    // Kích hoạt Animation và Hiệu ứng (Visual & Audio)
    // Tính toán sát thương và Áp dụng (Damage Calculation)
    // Áp dụng Hiệu ứng phụ (Buff/Debuff)
    public ExecutionState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }
}
