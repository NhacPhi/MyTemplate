using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class EnemyTurnState : BattleBaseState
{
    public EnemyTurnState(StateMachine<BattleState, BattleBaseState> stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        // Get enemy
        // Coroutie or Task to AI choice action
        // Enemy Executed action
    }

    public override void Exit()
    {

    }

    private IEnumerator ExecuteEnemyAction()
    {
        // Enemy action
        yield return new WaitForSeconds(1.0f);

        // check action complete and confidition (lose ? ) => swith to Lose State or EndTurn State
    }
}
