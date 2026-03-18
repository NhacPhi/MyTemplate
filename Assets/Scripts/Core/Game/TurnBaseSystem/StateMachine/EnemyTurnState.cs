using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnState : BattleBaseState
{
    public EnemyTurnState(BattleManager battleManager) : base(battleManager) { }

    public override async void Enter()
    {
        var brain = battleManager.CurrentCharacter.GetCoreComponent<EnemyBrain>();


        var playerTeam = battleManager.Characters.Values.ToList();


        EnemyDecision decision = await brain.DecideAsync(playerTeam);

        // 3. Setup dữ liệu để chuẩn bị chém
        battleManager.CurrentSkill = decision.SkillType;

        battleManager.SetCurrentTarget(decision.Target); 

        // 4. Mọi thứ đã chốt xong! Chuyển sang ExecutionState để Hàng đợi (Queue) bắt đầu múa!
        battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
    }
}
