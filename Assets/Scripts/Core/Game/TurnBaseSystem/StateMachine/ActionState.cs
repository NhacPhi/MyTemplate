using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class ActionState : BattleBaseState
{
    // If player: Bật UI quản lý Battle Scene lên. else enemy tắt CharacterUI chuyến sang ExecutetionState
    // player chọn skill và Target => chuyển sang thực thi với current skill và current Target (xử lý cho kill AOE )
    public ActionState(BattleManager battleManager) : base(battleManager) { }

    private bool _isHandlingAuto = false;

    public override void Enter()
    {
        _isHandlingAuto = false;
    }

    public override void Exit()
    {
        battleManager.IsExecutedAction = false;
    }

    public override async void OnUpdate()
    {
        if (BattleUIScene.IsAutoBattle && !_isHandlingAuto)
        {
            _isHandlingAuto = true;
            
            // Clean up UI manually since we skip manual action
            battleManager.TargetSystem.ResetTargetVisuals(battleManager.ActiveEntities);

            // Execute auto logic
            EnemyDecision decision = await battleManager.GeneratePlayerAutoDecisionAsync();
            
            battleManager.CurrentSkill = decision.SkillType;
            battleManager.SetCurrentTarget(decision.Target);
            battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
            return;
        }

        if(battleManager.IsExecutedAction && !_isHandlingAuto)
        {
            battleManager.TargetSystem.ResetTargetVisuals(battleManager.ActiveEntities);
            battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
        }
    }
}
