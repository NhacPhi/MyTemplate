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
    MAIN_SKILL,
    MAJOR_SKILL
}

public class EntityStateData : CoreComponent
{
    public StateMachine<EntityState, EntityStateBase> StateManager { get; protected set; }
    public Entity Entity { get; protected set; }
    public EntityStats StatsManager { get; protected set; }
    public AnimationSystemBase Anim { get; protected set; }
    // SkillManager;

    // param
    [field: SerializeField] public float TimeToReadyMoveAttack { get; protected set; } = 0.3f;
    [field: SerializeField] public float TimeToEndTurn { get; protected set; } = 0.3f;
    [field: SerializeField] public float MoveSpeed { get; protected set; } = 12f;

    [field: SerializeField] public float SmoothTime { get; protected set; } = 0.3f;
    [field: SerializeField] public Vector3 OffsetToTarget { get; protected set; } = Vector3.right * 5;
    public Vector3 RootPosition { get; protected set; }

    [NonSerialized] public Entity CurrentTarget;
    [NonSerialized] public Vector3 MovePosition;

    [field: Header("Animmation Parameters")]
    [field: SerializeField] public string IdleAnimation { get; protected set; } = "Idle";

    [field: SerializeField] public string AttackAnimation{ get; protected set; } = "Attack";
    [field: SerializeField] public string HitAnimation { get; protected set; } = "Injured";

    [field: SerializeField] public string MainSkillAnimaiton{ get; protected set; } = "UltimateSkill";

    [field: SerializeField] public string MajorAnimaiton { get; protected set; } = "MajorSkill";

    [field: SerializeField] public float RadioTimeTriggerDamgeMajor { get; protected set; } = 0.7f;

    [field: SerializeField] public float TimeTriggerDamge { get; protected set; } = 0.7f;

    [SerializeField] private AttackType attackType;


    // ==========================================
    private UniTaskCompletionSource _hitFrameTcs;
    private UniTaskCompletionSource _animEndTcs;
    private UniTaskCompletionSource _moveEndTcs;

    public UniTaskCompletionSource HitFrameTcs => _hitFrameTcs;
    public UniTaskCompletionSource AnimEndTcs => _animEndTcs;
    public UniTaskCompletionSource MoveEndTcs => _moveEndTcs;

    public UniTask WaitForHitFrame()
    {
        _hitFrameTcs = new UniTaskCompletionSource();
        return _hitFrameTcs.Task;
    }

    public UniTask WaitForAnimEnd()
    {
        _animEndTcs = new UniTaskCompletionSource();
        return _animEndTcs.Task;
    }

    public UniTask WaitForMoveEnd()
    {
        _moveEndTcs = new UniTaskCompletionSource();
        return _moveEndTcs.Task;
    }


    public void TriggerHitFrame()
    {
        _hitFrameTcs?.TrySetResult();
        _hitFrameTcs = null;
    }

    public void TriggerAnimEnd()
    {
        _animEndTcs?.TrySetResult();
        _animEndTcs = null;
    }

    public void TriggerMoveEnd()
    {
        _moveEndTcs?.TrySetResult();
        _moveEndTcs = null;
    }
    // ==========================================
    public CancellationToken Token => Entity.transform.GetCancellationTokenOnDestroy();
    public override void LoadComponent()
    {
        Entity = core as Entity;
        StateManager = Entity.StateManager;
        Anim = Entity.GetCoreComponent<AnimationSystemBase>();
        StatsManager = Entity.GetCoreComponent<EntityStats>();
        //RootPosition = Entity.transform.position;
    }

    public virtual void HandleTurn()
    {
        MovePosition = CurrentTarget.gameObject.transform.position - OffsetToTarget;
    }

    public void SetRootTransform()
    {
        RootPosition = Entity.transform.position;
    }    
}
