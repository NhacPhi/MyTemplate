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
        // Init UI
        // Spawn Units()
        // Setting Turn Order
        // Swtich State
        _ = LoadAllResourceBeforeStartBattle();
    }

    public override void Exit() 
    {
 
    }

    private async UniTaskVoid LoadAllResourceBeforeStartBattle()
    {
        var token = battleManager.DestroyCancellationToken;

        await battleManager.LoadEntitiesDataAsync(token);

        battleManager.SetupEntitiesPosition();
        battleManager.SetTarget();
    }

    public override void OnUpdate()
    {
       
    }
}
