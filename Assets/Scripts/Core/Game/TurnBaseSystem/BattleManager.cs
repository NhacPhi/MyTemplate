using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tech.StateMachine;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public StateMachine<BattleState, BattleBaseState> StateMachine;

    private List<Entity> _activeEntities = new List<Entity>();

    private List<Entity> _enemies = new List<Entity>();

    private Dictionary<string, Entity> _characters = new Dictionary<string, Entity>();

    private CancellationTokenSource cts = new CancellationTokenSource();
    public CancellationToken DestroyCancellationToken => cts.Token;

    [SerializeField] private List<Transform> _characterPosisions;
    [SerializeField] private List<Transform> _enemiesPositions;
    [SerializeField] public GameObject SeletionCircle;
    public List<Entity> ActiveEntities => _activeEntities;
    public List<Entity> AllEntities => _activeEntities;
    public Dictionary<string, Entity> Characters => _characters;
    public List<Entity> Enemies => _enemies; 

    [Inject] public SaveSystem SaveSystem { get; private set; }
    [Inject] public BattleSessionContext BattleSession { get; private set; }
    [Inject] public GameDataBase GameDataBase { get; private set; }
    [Inject] public UIManager UIManager { get; private set; }
    [Inject] public SceneLoader SceneLoader { get; private set; }
    [Inject] public InventoryManager InventoryManager { get; private set; }
    [Inject] public CurrencyManager CurrencyManager { get; private set; }

    [Inject] private EnemyManager _enemyManger;
    [Inject] private CharacterManager _characterManager;

    public float OffsetY = 4;

    public int GlobalTurnID { get; private set; } = 0;

    public void NextCharacterTurn()
    {
        GlobalTurnID++; // Lượt 1, Lượt 2, Lượt 3...
    }

    private Entity _currentCaster;

    private Entity _boss;
    private SkillCharacter _currentSkill;

    private BattleResult _resultBattle;
    public BattleResult ResultBattle
    {
        get { return _resultBattle; }
        set { _resultBattle = value; }
    }

    public async UniTask<EnemyDecision> GeneratePlayerAutoDecisionAsync()
    {
        await UniTask.Delay(500); // 0.5s thinking

        var skillManager = CurrentCaster.GetCoreComponent<EntitySkill>();
        var casterStats = CurrentCaster != null ? CurrentCaster.GetCoreComponent<EntityStats>() : null;
        bool isSilenced = casterStats != null && casterStats.IsSilenced();

        SkillCharacter chosenSkill = SkillCharacter.Base;
        Entity chosenTarget = null;

        var myTeam = GetEntitiesByTeam(CurrentCaster.Team);
        var opposingTeam = GetEntitiesByTeam(CurrentCaster.Team == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player);

        // -------------------------------------------------------------
        // BƯỚC 1: ƯU TIÊN SỐ 1 - KỸ NĂNG BUFF CHỈ SỐ / KHIÊN HỘ THỂ CHO BẢN THÂN / ĐỒNG ĐỘI
        // -------------------------------------------------------------
        if (!isSilenced && skillManager != null)
        {
            if (skillManager.Skills.ContainsKey(SkillCharacter.Ultimate) && skillManager.Skills[SkillCharacter.Ultimate].CurrentCooldown == 0 && EnemyBrain.IsSelfOrAllyBuffSkill(skillManager.Skills[SkillCharacter.Ultimate]))
            {
                var ultSkill = skillManager.Skills[SkillCharacter.Ultimate];
                var validTargets = TargetSystem.GetValidTargetsForSkill(ultSkill, CurrentCaster, myTeam, opposingTeam);
                var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                if (aliveTargets.Count > 0)
                {
                    chosenSkill = SkillCharacter.Ultimate;
                    chosenTarget = aliveTargets[0];
                }
            }
            else if (skillManager.Skills.ContainsKey(SkillCharacter.Major) && skillManager.Skills[SkillCharacter.Major].CurrentCooldown == 0 && EnemyBrain.IsSelfOrAllyBuffSkill(skillManager.Skills[SkillCharacter.Major]))
            {
                var majorSkill = skillManager.Skills[SkillCharacter.Major];
                var validTargets = TargetSystem.GetValidTargetsForSkill(majorSkill, CurrentCaster, myTeam, opposingTeam);
                var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                if (aliveTargets.Count > 0)
                {
                    chosenSkill = SkillCharacter.Major;
                    chosenTarget = aliveTargets[0];
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 2: ƯU TIÊN SỐ 2 - KỸ NĂNG HỒI MÁU (KHI CÓ ĐỒNG ĐỘI < 50% HP)
        // -------------------------------------------------------------
        if (chosenTarget == null && !isSilenced && skillManager != null)
        {
            Entity lowHpAlly = EnemyBrain.GetLowestHpAllyBelowThreshold(myTeam, 0.5f);
            if (lowHpAlly != null)
            {
                if (skillManager.Skills.ContainsKey(SkillCharacter.Ultimate) && skillManager.Skills[SkillCharacter.Ultimate].CurrentCooldown == 0 && EnemyBrain.IsHealingSkill(skillManager.Skills[SkillCharacter.Ultimate]))
                {
                    chosenSkill = SkillCharacter.Ultimate;
                    chosenTarget = lowHpAlly;
                }
                else if (skillManager.Skills.ContainsKey(SkillCharacter.Major) && skillManager.Skills[SkillCharacter.Major].CurrentCooldown == 0 && EnemyBrain.IsHealingSkill(skillManager.Skills[SkillCharacter.Major]))
                {
                    chosenSkill = SkillCharacter.Major;
                    chosenTarget = lowHpAlly;
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 2.5: ƯU TIÊN KỸ NĂNG ÁP ĐẶT DẤU ẤN / DEBUFF (SETUP SKILL)
        // Nếu sở hữu kỹ năng Major khắc Dấu Ấn / Debuff và đối phương chưa bị dính ấn, ưu tiên dùng trước để setup combo!
        // -------------------------------------------------------------
        if (chosenTarget == null && !isSilenced && skillManager != null)
        {
            if (skillManager.Skills.ContainsKey(SkillCharacter.Major) && skillManager.Skills[SkillCharacter.Major].CurrentCooldown == 0)
            {
                var majorSkill = skillManager.Skills[SkillCharacter.Major];
                if (EnemyBrain.IsMarkOrSetupSkill(majorSkill))
                {
                    var validTargets = TargetSystem.GetValidTargetsForSkill(majorSkill, CurrentCaster, Characters.Values.ToList(), Enemies);
                    var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                    
                    var effectType = majorSkill.GetSkillData().Effect.Type;
                    var unmarkedTarget = aliveTargets.FirstOrDefault(e => {
                        var stats = e.GetCoreComponent<EntityStats>();
                        return stats != null && !stats.StatusEffect.Any(eff => eff.Data != null && eff.Data.Type == effectType);
                    });

                    if (unmarkedTarget != null)
                    {
                        chosenSkill = SkillCharacter.Major;
                        chosenTarget = unmarkedTarget;
                    }
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 3: ƯU TIÊN SỐ 3 - TUYỆT KỸ ULTIMATE TẤN CÔNG / KHỐNG CHẾ
        // -------------------------------------------------------------
        if (chosenTarget == null && !isSilenced && skillManager != null && skillManager.Skills.ContainsKey(SkillCharacter.Ultimate) && skillManager.Skills[SkillCharacter.Ultimate].CurrentCooldown == 0)
        {
            var ultSkill = skillManager.Skills[SkillCharacter.Ultimate];
            var ultData = ultSkill.GetSkillData();

            // Nếu không phải là chiêu Heal đang chờ (vì Heal đã kiểm tra ở Bước 2)
            if (!EnemyBrain.IsHealingSkill(ultSkill))
            {
                var validTargets = TargetSystem.GetValidTargetsForSkill(ultSkill, CurrentCaster, Characters.Values.ToList(), Enemies);
                var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();

                if (aliveTargets.Count > 0)
                {
                    if (ultData != null && ultData.Effect != null && (ultData.Effect.Type == EffectType.Stun || ultData.Effect.Type == EffectType.Frozen))
                    {
                        // Chiêu Khống chế cứng: Ưu tiên mục tiêu chưa bị CC có ATK cao nhất
                        var nonCCed = aliveTargets.Where(e => e.GetCoreComponent<EntityStats>().CanTakeTurn()).OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
                        if (nonCCed != null)
                        {
                            chosenSkill = SkillCharacter.Ultimate;
                            chosenTarget = nonCCed;
                        }
                    }
                    else if (ultData != null && ultData.Effect != null && ultData.Effect.Type == EffectType.Silence)
                    {
                        // Chiêu Câm Lặng: Ưu tiên mục tiêu chưa bị Silence / CC
                        var validSil = aliveTargets.Where(e => e.GetCoreComponent<EntityStats>().CanTakeTurn() && !e.GetCoreComponent<EntityStats>().IsSilenced()).OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
                        if (validSil != null)
                        {
                            chosenSkill = SkillCharacter.Ultimate;
                            chosenTarget = validSil;
                        }
                    }
                    else
                    {
                        // Chiêu Sát Thương / AoE: Dồn vào mục tiêu thấp máu nhất để kết liễu
                        chosenSkill = SkillCharacter.Ultimate;
                        chosenTarget = aliveTargets.OrderBy(e => e.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value).FirstOrDefault();
                    }
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 4: ƯU TIÊN SỐ 4 - KỸ NĂNG MAJOR TẤN CÔNG / KHỐNG CHẾ
        // -------------------------------------------------------------
        if (chosenTarget == null && !isSilenced && skillManager != null && skillManager.Skills.ContainsKey(SkillCharacter.Major) && skillManager.Skills[SkillCharacter.Major].CurrentCooldown == 0)
        {
            var majorSkill = skillManager.Skills[SkillCharacter.Major];
            var majorData = majorSkill.GetSkillData();

            if (!EnemyBrain.IsHealingSkill(majorSkill))
            {
                var validTargets = TargetSystem.GetValidTargetsForSkill(majorSkill, CurrentCaster, Characters.Values.ToList(), Enemies);
                var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();

                if (aliveTargets.Count > 0)
                {
                    if (majorData != null && majorData.Effect != null && (majorData.Effect.Type == EffectType.Stun || majorData.Effect.Type == EffectType.Frozen))
                    {
                        var nonCCed = aliveTargets.Where(e => e.GetCoreComponent<EntityStats>().CanTakeTurn()).OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
                        if (nonCCed != null)
                        {
                            chosenSkill = SkillCharacter.Major;
                            chosenTarget = nonCCed;
                        }
                    }
                    else if (majorData != null && majorData.Effect != null && majorData.Effect.Type == EffectType.Silence)
                    {
                        var validSil = aliveTargets.Where(e => e.GetCoreComponent<EntityStats>().CanTakeTurn() && !e.GetCoreComponent<EntityStats>().IsSilenced()).OrderByDescending(e => e.GetCoreComponent<EntityStats>().GetStat(StatType.ATK)?.Value ?? 0f).FirstOrDefault();
                        if (validSil != null)
                        {
                            chosenSkill = SkillCharacter.Major;
                            chosenTarget = validSil;
                        }
                    }
                    else
                    {
                        chosenSkill = SkillCharacter.Major;
                        chosenTarget = aliveTargets.OrderBy(e => e.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value).FirstOrDefault();
                    }
                }
            }
        }

        // -------------------------------------------------------------
        // BƯỚC 5: ĐÒN ĐÁNH CƠ BẢN (BASE ATTACK) - KẾT LIỄU MỤC TIÊU THẤP MÁU
        // -------------------------------------------------------------
        if (chosenTarget == null)
        {
            chosenSkill = SkillCharacter.Base;
            var baseSkill = skillManager != null && skillManager.Skills.ContainsKey(SkillCharacter.Base) ? skillManager.Skills[SkillCharacter.Base] : null;
            if (baseSkill != null)
            {
                var validTargets = TargetSystem.GetValidTargetsForSkill(baseSkill, CurrentCaster, Characters.Values.ToList(), Enemies);
                var aliveTargets = validTargets.Where(e => e != null && e.GetCoreComponent<EntityStats>() != null && !e.GetCoreComponent<EntityStats>().IsDead).ToList();
                if (aliveTargets.Count > 0)
                {
                    chosenTarget = aliveTargets.OrderBy(e => e.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp).Value).FirstOrDefault();
                }
            }
        }

        return new EnemyDecision
        {
            SkillType = chosenSkill,
            Target = chosenTarget
        };
    }

    // Current pram
    public Entity CurrentCaster 
    { 
    
        get { return _currentCaster; }
        set { _currentCaster = value; }
    }
    public SkillCharacter CurrentSkill
    {

        get { return _currentSkill; }
        set { _currentSkill = value; }
    }

    public Entity Boss
    {
        get { return _boss; }
    }

    public List<Entity> GetEntitiesByTeam(TeamSide team)
    {
        if (team == TeamSide.Player)
        {
            return Characters != null ? Characters.Values.ToList() : new List<Entity>();
        }
        else
        {
            return Enemies != null ? Enemies : new List<Entity>();
        }
    }

    public bool IsExecutedAction = false;

    public TurnOrderSystem TurnSystem { get; private set; }
    public TargetManager TargetSystem { get; private set; }

    public Queue<Func<UniTask>> ActionQueue = new Queue<Func<UniTask>>();
    private void Start()
    {
        TurnSystem = new TurnOrderSystem();
        TargetSystem = new TargetManager();

        InitStateMachine();
    }

    private void OnEnable()
    {
        UIEvent.OnChooseSkillCharacter += SetupCurrentSkillCaster;
        UIEvent.OnChooseTargetEnemy += SetCurrentTarget;
        UIEvent.OnExecuteSkill += ExecuteSkillEntity;
    }

    private void OnDisable()
    {
        UIEvent.OnChooseSkillCharacter -= SetupCurrentSkillCaster;
        UIEvent.OnChooseTargetEnemy -= SetCurrentTarget;
        UIEvent.OnExecuteSkill -= ExecuteSkillEntity;
    }

    public void EnqueueAction(Func<UniTask> action)
    {
        ActionQueue.Enqueue(action);
    }
    public async UniTask LoadEntitiesDataAsync(CancellationToken cancellation = default)
    {
        var enemiesTask = _enemyManger.LoadAndSpawnEnemiesAsync(cancellation);
        var charactersTask = _characterManager.LoadAndSpawnCharactersAsync(cancellation);

        var (enemiesResult, charactersResult) = await UniTask.WhenAll(enemiesTask, charactersTask);

        this._enemies = enemiesResult;
        this._characters = charactersResult;
    }

    public void SetupEntitiesPosition()
    {
        // Load locaiton of character 
        var activeSlot = SaveSystem.Player.Roster.ActiveSlots;

        foreach (var slot in activeSlot)
        {
            if (!string.IsNullOrEmpty(slot.CharacterID))
            {
                var key = slot.CharacterID;
                var slot_position = slot.Position;
                int renderOrder = GetRenderOrderBySlot(slot_position);

                var character = _characters.GetValueOrDefault(key);
                if (character == null) continue;

                var pos = _characterPosisions[slot_position - 1].transform.position;

                SortingGroup sp = character.GetComponent<SortingGroup>();
                if (sp == null)
                {
                    sp = character.gameObject.AddComponent<SortingGroup>();
                }
                sp.sortingOrder = renderOrder;

                character.RenderOrder = renderOrder;

                character.Team = TeamSide.Player;

                character.transform.position = pos + Vector3.up * OffsetY;

                character.gameObject.GetComponent<EntityStateData>().SetRootTransform();

                SetEntityPositionBySlot(character, slot_position);
            }         
        }

        var battleConfig = GameDataBase.GetBattleConfig(BattleSession.PendingBattleID);

        for (int i = 0; i < _enemies.Count; i++)
        {
            var slot_position = battleConfig.Enemies[i].Slot;
            int renderOrder = GetRenderOrderBySlot(slot_position);

            SetEntityPositionBySlot(Enemies[i], slot_position);

            var enemy = _enemies[i];
            var pos = _enemiesPositions[slot_position - 1].transform.position;
            enemy.transform.position = pos + Vector3.up * OffsetY;
            enemy.gameObject.GetComponent<EntityStateData>().SetRootTransform();
            SortingGroup sp = enemy.GetComponent<SortingGroup>();
            if (sp == null)
            {
                sp = enemy.gameObject.AddComponent<SortingGroup>();
            }
            sp.sortingOrder = renderOrder;

            enemy.RenderOrder = renderOrder;

            enemy.Team = TeamSide.Enemy;
        }
    }

    public static int GetRenderOrderBySlot(int slot)
    {
        // Slot 1 & 4 (Hàng Trên Cùng - Y cao nhất): Sorting Layer 2 (Vẽ phía sau)
        // Slot 2 & 5 (Hàng Ở Giữa - Y trung bình):  Sorting Layer 4 (Vẽ ở giữa)
        // Slot 3 & 6 (Hàng Dưới Cùng - Y thấp nhất): Sorting Layer 6 (Vẽ đè lên trước)
        int rowInGrid = ((slot - 1) % 3) + 1;
        return rowInGrid * 2;
    }

    public void SetEntityPositionBySlot(Entity entity, int slot)
    {
        entity.Row = (slot <= 3) ? BattleRow.Front : BattleRow.Back;

        int columnIndex = (slot - 1) % 3;
        entity.Column = (BattleColumn)columnIndex;
    }

    public void CheckBattleHasBosss()
    {
        _boss = null;
        var battleConfig = GameDataBase.GetBattleConfig(BattleSession.PendingBattleID);

        for (int i = 0; i < battleConfig.Enemies.Count; i++)
        {
            if (battleConfig.Enemies[i].IsBoss && i < _enemies.Count)
            {
                _boss = _enemies[i];
                break;
            }
        }
    }

    public void ExecuteSkillEntity()
    {
        IsExecutedAction = true;
    }

    public void SetupCurrentSkillCaster(SkillCharacter type)
    {
        _currentSkill = type;

        if (_currentCaster == null) return;

        // Chỉ cho phép highlight / làm mờ mục tiêu khi đang ở lượt chọn kỹ năng của Player (ActionState / BeginTurnBase)
        if (StateMachine != null && StateMachine.CurrentState != null && !(StateMachine.CurrentState is ActionState || StateMachine.CurrentState is BeginTurnBase))
        {
            return;
        }

        var entitySkill = _currentCaster.GetCoreComponent<EntitySkill>();
        if (entitySkill == null || entitySkill.Skills == null) return;

        var skill = entitySkill.Skills.GetValueOrDefault(_currentSkill);

        if(skill != null)
        {
            var validTargets = TargetSystem.GetValidTargetsForSkill(skill, CurrentCaster, _characters.Values.ToList(), _enemies);

            TargetSystem.HighlightTargets(_activeEntities, validTargets);
        }

        _currentCaster.SetRenderValid(true);
    }
    
    public void SetCurrentTarget(Entity target)
    {
        if (_currentCaster == null) return;

        var entitySkill = _currentCaster.GetCoreComponent<EntitySkill>();
        if (entitySkill == null || entitySkill.Skills == null) return;

        var skillRuntime = entitySkill.Skills.GetValueOrDefault(_currentSkill);
        if (skillRuntime == null || skillRuntime.GetSkillData() == null) return;

        SkillTargetType type = skillRuntime.GetSkillData().TargetType;
        _currentCaster.SetTargets(TargetSystem.GetTargets(_currentCaster, type, target, _activeEntities));
    }

    private void InitStateMachine()
    {
        StateMachine = new StateMachine<BattleState, BattleBaseState>();
        //SetupState, // Init data, spawn character, load inviroment
        //OrderState, // Decide player or enemy to do action. 
        //BeginTurnBase, // Handle effect or buff
        //ActionState, // Player Chosce SkillCharacter or AI controller
        //ExecutionState, // Run skill animation, caculate damaage
        //EndTurnState, // Check Win or Lose Condition, handle skill cooldown
        //ResultState // Show UI for result of battle (handle reward)
        StateMachine.AddNewState(BattleState.SetupState, new BattleSetupState(this));
        StateMachine.AddNewState(BattleState.OrderState, new OrderState(this));
        StateMachine.AddNewState(BattleState.BeginTurnBase, new BeginTurnBase(this));
        //StateMachine.AddNewState(BattleState.EnemyTurnsState, new EnemyTurnState(this));
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

    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
        if (Instance == this)
        {
            Instance = null;
        }
        Time.timeScale = 1f;
        BattleUIScene.CurrentSpeed = 1f;
    }

    public void Update()
    {
        StateMachine.CurrentState.OnUpdate();
    }

    public void RegisterEnemy(Entity enemy)
    {
        _enemies.Add(enemy);
    }

    public void RegisterCharacter(string key, Entity character)
    {
        _characters.Add(key, character);
    }
}
