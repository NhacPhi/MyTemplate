using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class BattleSetupState : BattleBaseState
{
    public BattleSetupState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        UIEvent.OnSwithActiveSkilCharacter?.Invoke(false);
        _ = LoadAllResourceBeforeStartBattle();
    }

    public override void Exit() 
    {
 
    }

    private async UniTask LoadAllResourceBeforeStartBattle()
    {
        var token = battleManager.DestroyCancellationToken;

        await battleManager.LoadEntitiesDataAsync(token);

        battleManager.SetupEntitiesPosition();

        battleManager.ActiveEntities.Clear();
        battleManager.ActiveEntities.AddRange(battleManager.Characters.Values);
        battleManager.ActiveEntities.AddRange(battleManager.Enemies);

        battleManager.TurnSystem.Inititalize(battleManager.ActiveEntities);

        // Reset cooldowns for all active entities when entering or restarting battle
        foreach (var entity in battleManager.ActiveEntities)
        {
            var entitySkill = entity.GetComponent<EntitySkill>();
            if (entitySkill != null)
            {
                entitySkill.ResetAllCooldowns();
            }
        }

        battleManager.CheckBattleHasBosss();

        UIEvent.OnActiveBossUI?.Invoke(battleManager.Boss != null);
        if (battleManager.Boss != null)
        {
            UIEvent.OnUpdateBossUI?.Invoke(battleManager.Boss);
        }

        // Update Skill UI for the first character in the line up (based on actual turn order)
        Entity firstCharacter = null;
        var predictedOrder = battleManager.TurnSystem.PredictTurnOrder();
        UIEvent.OnUpdateEntityPrediction?.Invoke(predictedOrder);

        foreach (var entity in predictedOrder)
        {
            if (entity.Team == TeamSide.Player)
            {
                firstCharacter = entity;
                break;
            }
        }
        if (firstCharacter != null)
        {
            UIEvent.OnUpdateSkillCharacterUI?.Invoke(firstCharacter);
            UIEvent.OnSwithActiveSkilCharacter?.Invoke(true);
        }

        battleManager.ResultBattle = BattleResult.Flee;

        await UniTask.Delay(1000, ignoreTimeScale: true, cancellationToken: token);

        battleManager.StateMachine.ChangeState(BattleState.OrderState);
    }

    public override void OnUpdate()
    {
       
    }
}
