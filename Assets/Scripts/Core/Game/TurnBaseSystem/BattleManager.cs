using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.StateMachine;
using System;
using VContainer.Unity;
using System.Threading;

public class BattleManager : MonoBehaviour, IInitializable, IDisposable, ITickable
{
    public StateMachine<BattleState, BattleBaseState> StateMachine;
    public List<Entity> activeEntities;

    private CancellationTokenSource cts = new CancellationTokenSource();
    public CancellationToken DestroyCancellationToken => cts.Token;


    // List character List<Entity> characters;
    // List Enemy List<Entity> enemies;

    // [Inject]
    public void Initialize()
    {
        InitStateMachine();
    }

    private void InitStateMachine()
    {
        StateMachine = new StateMachine<BattleState, BattleBaseState>();
        //SetupState, // Init data, spawn character, load inviroment
        //OrderState, // Decide order turn
        //BeginTurnBase, // Handle effect or buff
        //ActionState, // Player Chosce Skill or AI controller
        //ExecutionState, // Run skill animation, caculate damaage
        //EndTurnState, // Check Win or Lose Condition, handle skill cooldown
        //ResultState // Show UI for result of battle (handle reward)
        StateMachine.AddNewState(BattleState.SetupState, new BattleSetupState(this));
        StateMachine.AddNewState(BattleState.OrderState, new OrderState(this));
        StateMachine.AddNewState(BattleState.BeginTurnBase, new BeginTurnBase(this));
        StateMachine.AddNewState(BattleState.ActionState, new ActionState(this));
        StateMachine.AddNewState(BattleState.ExecutionState, new ExecutionState(this));
        StateMachine.AddNewState(BattleState.EndTurnState, new EndTurnState(this));
        StateMachine.AddNewState(BattleState.ResultState, new ResultState(this));

        // Setup battle
        StateMachine.Initialize(BattleState.SetupState);
    }

    public void Dispose()
    {
        // Remove Register event
    }

    public void Tick()
    {
        StateMachine.CurrentState.OnUpdate();
    }
}
