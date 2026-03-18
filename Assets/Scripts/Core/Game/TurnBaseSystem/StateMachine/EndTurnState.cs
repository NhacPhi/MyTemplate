using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;
using System.Linq;

public class EndTurnState : BattleBaseState
{
    // Trừ thời gian của Buff / Debuff (Duration Tick)
    //Reset các chỉ số tạm thời
    public EndTurnState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        bool isAllEnemiesDead = battleManager.Enemies.All(e => e.GetCoreComponent<EntityStats>().IsDead);

        if(isAllEnemiesDead)
        {
            battleManager.ResultBattle = BattleResult.Win;
            battleManager.StateMachine.ChangeState(BattleState.ResultState);
            return;
        }

        bool isAllPlayersDead = battleManager.Characters.Values
            .All(e => e.GetCoreComponent<EntityStats>().IsDead);

        if (isAllPlayersDead)
        {
            battleManager.ResultBattle = BattleResult.Lose;
            battleManager.StateMachine.ChangeState(BattleState.ResultState);
            return;
        }

        battleManager.TurnSystem.ResetEntityAV(battleManager.CurrentCharacter);
        battleManager.StateMachine.ChangeState(BattleState.OrderState);
    }

    public override void Exit()
    {

    }
}
