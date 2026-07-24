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

            if (battleManager != null && battleManager.Boss != null)
            {
                UIEvent.OnUpdateBossUI?.Invoke(battleManager.Boss);
            }

            battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
        }
        catch (System.OperationCanceledException)
        {
            // Object/Scene feature was canceled or destroyed normally. Clean exit.
            return;
        }
        catch (UnityEngine.MissingReferenceException)
        {
            // Object was destroyed mid-execution (scene unload / restart). Clean exit.
            return;
        }
        catch (System.ObjectDisposedException)
        {
            return;
        }
        catch (System.Exception e)
        {
            string casterName = (battleManager != null && battleManager.CurrentCaster != null) ? battleManager.CurrentCaster.name : "Unknown";
            Debug.LogError($"[ExecutionState] Lỗi khi thực thi skill của '{casterName}': {e}");
            if (battleManager != null && battleManager.StateMachine != null)
            {
                battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
            }
        }
    }

    public override void Exit()
    {

    }

    public override void OnUpdate()
    {

    }
}
