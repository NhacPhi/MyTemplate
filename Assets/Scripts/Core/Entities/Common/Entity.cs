using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Composite;
using Tech.StateMachine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

using System;

public enum BattleRow { Front, Back }
public enum BattleColumn { Left, Center, Right }

public enum TeamSide { Player, Enemy }
public abstract class Entity : Tech.Composite.Core, ITurn
{
    //Test
    public List<GameObject> Targets { get; protected set; } = new List<GameObject>(); // Could be Enemy or Ally

    public GameObject Target => Targets[0];

    public int RenderOrder = 0;

    public TeamSide Team;

    // --- BATTLE EVENTS ---
    public Action<Entity> OnTurnStart;
    public Action<Entity> OnTurnEnd;
    public Action<Entity, Entity, float, HashSet<string>> OnBeforeDealDamage;
    public Action<Entity, Entity, float, HashSet<string>> OnAfterDealDamage;
    public Action<Entity> OnAfterSkillExecute;

    // Turn Lifecycle
    // OnTurnStart buff/debuff
    // OnTurnEnd 

    // Skill/Action event
    // OnBeforeSkillExecute (Kiểm tra điều khiện kích hoạt pass ví dụ đơn mục tiêu hay diện rộng
    // OnAfterSkillExecute (Sau khi thi triển kĩ năng nhân vật có ....ap dụng các hiệu ứng nên bản thân)

    // Damage & Combat Event
    // OnBeforeDealDamage (Đòn tấn công có cơ hội giảm phòng thủ đối phương, tăng tỉ lệ chí mạng)
    // OnAfterDealDamage (Hồi máu cho nhân vật dựa vào lượng sát thương gây ra hiệu ứng hút máu)
    // OnBeforeTakeDamge (Kích hoạt khiên giảm sát thương nhận vào)
    // OnAfterTakeDamage (Kích hoạt phản sát thương)

    // Kill & Death Envent
    // OnKillTarget Mục tiêu bị hạ gục
    // OnBeforeDaeth Các passive hồi sinh (hồi sinh với 20%HP, hoặc nổ gây sát thương ra xung quanh)

    // Buff/Debuff Event Addvance
    // OnBuffApplied / OnDebuffApplied
    // OnBuffRemoved / OnDebuffRemoved


    [Header("Position info")]
    public BattleRow Row;
    public BattleColumn Column;
    // Statte Machine
    public StateMachine<EntityState, EntityStateBase> StateManager { get; protected set; }
    = new StateMachine<EntityState, EntityStateBase>();

    //public IEntityAttack attack { get; private set; }
    public IAudioManager Audio { get; private set; }


    [Inject]
    public void Construct(IAudioManager audioManager)
    {
        Audio = audioManager;
    }

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
        
        OnTurnStart?.Invoke(this);
        
        entityStateData.HandleTurn();
    }

    public void SetTarget(Entity enemy)
    {
        Targets.Clear();
        if (enemy != null) Targets.Add(enemy.gameObject);
    }

    public void SetTargets(List<Entity> enemies)
    {
        Targets.Clear();
        foreach (var e in enemies) Targets.Add(e.gameObject);
    }

    public virtual async UniTask ExecuteSkillAsync(SkillCharacter type, int currentTurnID)
    {
        await gameObject.GetComponent<EntitySkill>().ExecuteSkillAsync(type, currentTurnID);
        
        // Báo cho hệ thống biết kĩ năng đã thi triển xong (Để kích hoạt các Passive)
        OnAfterSkillExecute?.Invoke(this);
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

    public void SetTargetableVisual(bool isTargetable)
    {

        SetRenderValid(isTargetable);

        var hitBox = GetComponentInChildren<TargetHitbox>();
        if (hitBox != null)
        {
            var collider = hitBox.GetComponent<Collider>();
            if(collider != null)
            {
                collider.enabled = isTargetable;
                hitBox.SetTargetVisual(isTargetable);
            }
        }
    }

    public void ResetTargetVisual()
    {
        SetRenderValid(true);

        var hitBox = GetComponentInChildren<TargetHitbox>();

        if (hitBox != null)
        {
            var collider = hitBox.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                hitBox.SetTargetVisual(false);
            }
        }
    }

    public void SetRenderValid(bool valid)
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();

        Color displayColor = valid ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);

        foreach (var sr in allRenderers)
        {
            // Bỏ qua không đổi màu cho các Object là hiệu ứng (VFX, Eff) đính kèm trên nhân vật
            string nameLower = sr.gameObject.name.ToLower();
            if (nameLower.Contains("eff") || nameLower.Contains("vfx"))
            {
                continue;
            }

            sr.color = displayColor;
        }
    }

    public void PlaySFX(string soundID)
    {
        if (Audio != null && soundID != null)
        {
            Audio.PlaySFXAsync(soundID).Forget();
        }
    }
}
