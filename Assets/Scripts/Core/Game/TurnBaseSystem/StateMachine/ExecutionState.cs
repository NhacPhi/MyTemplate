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
            await battleManager.CurrentCaster.ExecuteSkillAsync(battleManager.CurrentSkill, battleManager.GlobalTurnID);
        });

        try
        {
            while (battleManager.ActionQueue.Count > 0)
            {
                var nextAction = battleManager.ActionQueue.Dequeue();
                await nextAction.Invoke();
            }

            if (battleManager == null || battleManager.CurrentCaster == null)
                return;

            Dictionary<SkillCharacter, SkillRuntime> activeSkills = battleManager.CurrentCaster.GetCoreComponent<EntitySkill>().Skills;

            battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
        }
        catch (System.Exception e)
        {
            // If the scene unloads or object is destroyed mid-execution, we safely abort the state transition.
            return;
        }
    }

    public override void Exit()
    {

    }

    public override void OnUpdate()
    {

    }
}
