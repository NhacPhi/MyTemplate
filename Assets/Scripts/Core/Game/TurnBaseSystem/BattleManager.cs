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
    public Dictionary<string, Entity> Characters => _characters;
    public List<Entity> Enemies => _enemies; 

    [Inject] private SaveSystem _saveSystem;
    [Inject] private BattleSessionContext _battleSession;
    [Inject] private GameDataBase _gameDataBase;

    [Inject] private EnemyManager _enemyManger;
    [Inject] private CharacterManager _characterManager;

    public float OffsetY = 4;

    private Entity _currentCharacter;
    private Entity _currentEnemy;
    private Entity _boss;
    private SkillCharacter _currentSkill;

    private BattleResult _resultBattle;
    public BattleResult ResultBattle
    {
        get { return _resultBattle; }
        set { _resultBattle = value; }
    }

    // Current pram
    public Entity CurrentCharacter 
    { 
    
        get { return _currentCharacter; }
        set { _currentCharacter = value; }
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

    public Entity CurrentEnemy
    {
        get { return _currentEnemy; }
        set { _currentEnemy = value; }
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
        UIEvent.OnChooseSkillCharacter += SetupCurrentSkillCharacter;
        UIEvent.OnChooseTargetEnemy += SetCurrentTarget;
        UIEvent.OnExecuteSkill += ExecuteSkillEntity;
    }

    private void OnDisable()
    {
        UIEvent.OnChooseSkillCharacter -= SetupCurrentSkillCharacter;
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
        var activeSlot = _saveSystem.Player.ActiveSlots;

        int order = 0;

        foreach(var slot in activeSlot)
        {
            if(order == 6)
            {
                order = 0;
            }

            order += 2;

            if (slot.CharacterID != "")
            {
                var key = slot.CharacterID;
                var slot_position = slot.Position;

                var character = _characters.GetValueOrDefault(key);
                var pos = _characterPosisions[slot_position - 1].transform.position;

                if (character.GetComponent<SortingGroup>() == null)
                {
                    SortingGroup sp = character.gameObject.AddComponent<SortingGroup>();

                    sp.sortingOrder = order;

                    character.gameObject.GetComponent<Entity>().RenderOrder = order;
                }
                character.transform.position = pos + Vector3.up * OffsetY;

                character.gameObject.GetComponent<EntityStateData>().SetRootTransform();

                SetEntityPositionBySlot(character, slot_position);
            }         
        }

        var battleConfig = _gameDataBase.GetBattleConfig(_battleSession.PendingBattleID);

        order = 0;
        for(int i = 0; i < _enemies.Count; i++)
        {
            SetEntityPositionBySlot(Enemies[i], battleConfig.Enemies[i].Slot);
            if (order == 6)
            {
                order = 0;
            }

            order += 2;
            //var key = enemyConfig.EnemyID;
            var slot_position = battleConfig.Enemies[i].Slot;
            var enemy = _enemies[i];
            var pos = _enemiesPositions[slot_position - 1].transform.position;
            enemy.transform.position = pos + Vector3.up * OffsetY;
            enemy.gameObject.GetComponent<EntityStateData>().SetRootTransform();
            if (enemy.GetComponent<SortingGroup>() == null)
            {
                SortingGroup sp = enemy.gameObject.AddComponent<SortingGroup>();

                sp.sortingOrder = order;

                enemy.gameObject.GetComponent<Entity>().RenderOrder = order;
            }
        }
    }

    public void SetEntityPositionBySlot(Entity entity, int slot)
    {
        entity.Row = (slot <= 3) ? BattleRow.Front : BattleRow.Back;

        int columnIndex = (slot - 1) % 3;
        entity.Column = (BattleColumn)columnIndex;
    }

    public void CheckBattleHasBosss()
    {
        foreach(var enemy in _enemies)
        {
            var bossBrain = enemy.GetCoreComponent<BossBrain>();

            if(bossBrain != null)
            {
                _boss = enemy;
                return;
            }
        }
    }

    public void ExecuteSkillEntity()
    {
        IsExecutedAction = true;
    }

    public void SetupCurrentSkillCharacter(SkillCharacter type)
    {
        _currentSkill = type;

        var skill = _currentCharacter.GetCoreComponent<EntitySkill>().Skills.GetValueOrDefault(_currentSkill);

        if(skill != null)
        {
            var validTargets = TargetSystem.GetValidTargetsForSkill(skill, CurrentCharacter, _characters.Values.ToList(), _enemies);

            TargetSystem.HighlightTargets(_activeEntities, validTargets);
        }

        _currentCharacter.SetRenderValid(true);
    }
    
    public void SetCurrentTarget(Entity target)
    {
        _currentEnemy = target;

        _currentCharacter.SetTaget(CurrentEnemy);
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
        StateMachine.AddNewState(BattleState.EnemyTurnsState, new EnemyTurnState(this));
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
