using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tech.StateMachine;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BeginTurnBase : BattleBaseState
{
    // Effect Xử lý sát thương / Hồi phục theo thời gian
    //Kiểm tra hiệu ứng khống chế (Crowd Control)
    //Kích hoạt Kỹ năng Bị động (Passive Skills)
    public BeginTurnBase(BattleManager battleManager) : base(battleManager) { }

    public override async void Enter()
    {
        // Effect Xử lý sát thương / Hồi phục theo thời gian
        var stats = battleManager.CurrentCaster.GetCoreComponent<StatsController>();
        bool skipTurn = !stats.CanTakeTurn();
        stats.ProcessStartOfTurn();

        if(battleManager.CurrentCaster.GetCoreComponent<EntityStats>().IsDead)
        {
            battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
            return;
        }

        // 4. KIỂM TRA KHỐNG CHẾ (CHOÁNG, NGỦ, ĐÓNG BĂNG...)
        if (skipTurn)
        {
            battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
            return;
        }


        // Skil 
        var enemyBrain = battleManager.CurrentCaster.GetCoreComponent<EnemyBrain>();

        var skillManager = battleManager.CurrentCaster.GetCoreComponent<EntitySkill>();

        if (skillManager != null)
        {
            skillManager.TickCooldowns();

            await UniTask.Delay(1000);
        }

        if (battleManager.Boss)
            UIEvent.OnUpdateBossUI(battleManager.Boss);

        UIEvent.OnUpdateEntityPrediction?.Invoke(battleManager.TurnSystem.PredictTurnOrder());

        if (enemyBrain != null )
        {
            //Enemy
            UIEvent.OnSwithActiveSkilCharacter?.Invoke(false);

            var playerTeam = battleManager.TargetSystem.GetValidEtitiesByColumnLogic(battleManager.Characters.Values.ToList());


            EnemyDecision decision = await enemyBrain.DecideAsync(playerTeam);

            // 3. Setup dữ liệu để chuẩn bị chém
            battleManager.CurrentSkill = decision.SkillType;

            battleManager.SetCurrentTarget(decision.Target);

            battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
        }
        else
        {
            //player

            UIEvent.OnSwithActiveSkilCharacter?.Invoke(true);

            UIEvent.OnUpdateSkillCharacterUI?.Invoke(battleManager.CurrentCaster);

            battleManager.SetupCurrentSkillCaster(SkillCharacter.Base);

            battleManager.StateMachine.ChangeState(BattleState.ActionState);
        }
    }

    public override void Exit()
    {

    }
}
