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

    public override async void Enter()
    {
        battleManager.EnqueueAction(async () =>
        {
            await battleManager._CurrentCharacter.ExecuteSkillAsync(battleManager._CurrentSkill);
        });

        while (battleManager.ActionQueue.Count > 0)
        {
            var nextAction = battleManager.ActionQueue.Dequeue();
            await nextAction.Invoke();
        }

        battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
    }

    public override void Exit()
    {

    }

    public override void OnUpdate()
    {

    }
}
