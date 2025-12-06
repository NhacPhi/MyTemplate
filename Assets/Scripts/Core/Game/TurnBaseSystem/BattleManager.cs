using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.StateMachine;
using System;
using VContainer.Unity;

public enum BattleState
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    TurnEnd,
    BattleWon,
    BattleLose
}

public class BattleManager : IInitializable, IDisposable, ITickable
{
    private StateMachine<BattleState, BattleBaseState> stateMachine;

    // [Inject]
    public void Initialize()
    {

    }
    public void Dispose()
    {
        // Remove Register event
    }

    public void Tick()
    {
        stateMachine.CurrentState.OnUpdate();
    }
}
