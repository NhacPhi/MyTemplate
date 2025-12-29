using System;
using UnityEngine;
using Tech.Composite;
using Tech.StateMachine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading;

public enum EntityState
{
    IDLE,
    ATTACK,
    HIT,
    MOVE_UP,
    MOVE_DOWN,
    MAIN_SKILL
}

public class EntityStateData : CoreComponent
{
    public StateMachine<EntityState, EntityStateBase> StateManager { get; protected set; }
    public EntityState NextStateAfterMoveext { get; set; } = EntityState.ATTACK;
    public Entity Entity { get; protected set; }
    public EntityStats StatsManager { get; protected set; }
    public AnimationSystemBase Anim { get; protected set; }
    // SkillManager;

    // param
    [field: SerializeField] public float TimeToReadyMoveAttack { get; protected set; } = 0.3f;
    [field: SerializeField] public float TimeToAttack { get; protected set; } = 1f;
    [field: SerializeField] public float TimeToEndTurn { get; protected set; } = 0.3f;
    [field: SerializeField] public float MoveSpeed { get; protected set; } = 8f;
    [field: SerializeField] public Vector3 OffsetToTarget { get; protected set; } = Vector3.right * 5;
    public Vector3 RootPosition { get; protected set; }

    [NonSerialized] public Entity CurrentTarget;
    [NonSerialized] public Vector3 MovePosition;

    [field: Header("Animmation Parameters")]
    [field: SerializeField] public string IdleParam { get; protected set; } = "Idle";
    //[field: SerializeField] public string MoveAnim { get; protected set; } = "Move";
    [field: SerializeField] public string AttackParam { get; protected set; } = "Attack";
    [field: SerializeField] public string HitParam { get; protected set; } = "Injured";

    [field: SerializeField] public string MainSkillParam { get; protected set; } = "UltimateSkill";

    public CancellationToken Token => Entity.transform.GetCancellationTokenOnDestroy();
    public override void LoadComponent()
    {
        Entity = core as Entity;
        StateManager = Entity.StateManager;
        Anim = Entity.GetCoreComponent<AnimationSystemBase>();
        StatsManager = Entity.GetCoreComponent<EntityStats>();
        RootPosition = Entity.transform.position;
    }

    public virtual void HandleTurn()
    {
        _ = WaitToReadyAttack();
    }

    protected virtual async UniTaskVoid WaitToReadyAttack()
    {
        await UniTask.WaitForSeconds(TimeToReadyMoveAttack, cancellationToken: Token);
        MovePosition = CurrentTarget.gameObject.transform.position - OffsetToTarget;
        StateManager.ChangeState(EntityState.MOVE_UP);
    }
}
