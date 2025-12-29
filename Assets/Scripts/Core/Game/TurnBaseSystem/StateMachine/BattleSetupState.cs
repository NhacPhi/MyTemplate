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

        List<UniTask> loadTasks = new List<UniTask>();


        foreach(Entity entity in battleManager.activeEntities)
        {
            EntitySkill skill = entity.gameObject.GetComponent<EntitySkill>();

            loadTasks.Add(skill.InitializeAsync(token));
        }

        await UniTask.WhenAll(loadTasks);

    }
}
