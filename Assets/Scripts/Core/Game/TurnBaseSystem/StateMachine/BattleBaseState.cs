using UnityEngine;
using Tech.StateMachine;

public enum BattleState
{
    SetupState, // Init data, spawn character, load inviroment
    OrderState, // Decide order turn
    BeginTurnBase, // Handle effect or buff
    ActionState, // Player Chosce Skill or AI controller
    ExecutionState, // Run skill animation, caculate damaage
    EndTurnState, // Check Win or Lose Condition, handle skill cooldown
    ResultState // Show UI for result of battle (handle reward)
}
public abstract class BattleBaseState : BaseState
{
    protected BattleManager battleManager;

    public BattleBaseState(BattleManager battleManager)
    {
        this.battleManager = battleManager;
    }
    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public virtual void OnUpdate() { }
}
