using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Composite;
using Tech.StateMachine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;

public abstract class Entity : Tech.Composite.Core, ITurn
{
    //Test
    public GameObject Target;

    public int RenderOrder = 0;
    // Statte Machine
    public StateMachine<EntityState, EntityStateBase> StateManager { get; protected set; }
    = new StateMachine<EntityState, EntityStateBase>();

    //public IEntityAttack attack { get; private set; }


    protected EntityStateData entityStateData;
    protected override void LoadComponent()
    {
        InitStateMachine();
        //switch (attackType)
        //{
        //    case AttackType.Melee:
        //        attack = new EntityMeleeAttack();
        //        break;
        //    case AttackType.Range:
        //        attack = new EntityRangeAttack();
        //        break;
        //}

    }

    protected virtual void InitStateMachine()
    {
        entityStateData = GetCoreComponent<EntityStateData>();
        AddStateToStateMachine(entityStateData);
    }

    protected virtual void AddStateToStateMachine(EntityStateData entityStateData)
    {
        StateManager.AddNewState(EntityState.IDLE, new EntityIdle(entityStateData));
        StateManager.AddNewState(EntityState.ATTACK, new EntityAttack(entityStateData));
        StateManager.AddNewState(EntityState.HIT, new EntityTakeHit(entityStateData));
        StateManager.AddNewState(EntityState.MOVE_UP, new EntityMoveUp(entityStateData));
        StateManager.AddNewState(EntityState.MOVE_DOWN, new EntityMoveDown(entityStateData));
        StateManager.AddNewState(EntityState.MAIN_SKILL, new EntityMainSkill(entityStateData));
        StateManager.AddNewState(EntityState.MAJOR_SKILL, new EntityMajorSkill(entityStateData));
    }

    protected virtual void Start()
    {
        StateManager.Initialize(EntityState.IDLE);
    }

    protected virtual void Update()
    {
        StateManager.CurrentState.LogicUpdate();
    }
    public bool IsEndTurn { get; set; }
    public virtual void HandleTurn(Entity target)
    {
        IsEndTurn = false;
        entityStateData.CurrentTarget = target;
        entityStateData.HandleTurn();
    }

    public void SetTaget(Entity enemy)
    {
        Target = enemy.gameObject;
    }

    public virtual async UniTask ExecuteSkillAsync(SkillCharacter type)
    {
        await gameObject.GetComponent<EntitySkill>().ExecuteSkillAsync(type);
    }

    public string GetEntityID()
    {
        return GetComponent<EntityStats>().EntityID;
    }


    // Render order
    public void SetRenderOrder(int order)
    {
        var sortingGP = gameObject.GetComponent<SortingGroup>();

        if(sortingGP != null)
        {
            sortingGP.sortingOrder = order;
        }
    }
}
