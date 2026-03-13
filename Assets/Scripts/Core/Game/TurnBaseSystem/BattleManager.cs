using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.StateMachine;
using System;
using VContainer.Unity;
using System.Threading;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;
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
    public List<Entity> ActiveEntities => _activeEntities;

    [Inject] private SaveSystem _saveSystem;
    [Inject] private BattleSessionContext _battleSession;
    [Inject] private GameDataBase _gameDataBase;

    [Inject] private EnemyManager _enemyManger;
    [Inject] private CharacterManager _characterManager;

    float OffsetY = 4;

    private void Start()
    {
        InitStateMachine();
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
        foreach(var slot in activeSlot)
        {
            if(slot.CharacterID != "")
            {
                var key = slot.CharacterID;
                var slot_position = slot.Position;

                var character = _characters.GetValueOrDefault(key);
                var pos = _characterPosisions[slot_position - 1].transform.position;

                if (character.GetComponent<SortingGroup>() == null)
                {
                    SortingGroup sp = character.gameObject.AddComponent<SortingGroup>();

                    sp.sortingOrder = slot_position;
                }
                character.transform.position = pos + Vector3.up * OffsetY;

                character.gameObject.GetComponent<EntityStateData>().SetRootTransform();
            }         
        }

        var battleConfig = _gameDataBase.GetBattleConfig(_battleSession.PendingBattleID);

        for(int i = 0; i < _enemies.Count; i++)
        {
            //var key = enemyConfig.EnemyID;
            var slot_position = battleConfig.Enemies[i].Slot;
            var enemy = _enemies[i];
            var pos = _enemiesPositions[slot_position - 1].transform.position;
            enemy.transform.position = pos + Vector3.up * OffsetY;
            enemy.gameObject.GetComponent<EntityStateData>().SetRootTransform();
            if (enemy.GetComponent<SortingGroup>() == null)
            {
                SortingGroup sp = enemy.gameObject.AddComponent<SortingGroup>();

                sp.sortingOrder = slot_position;
            }
        }
    }

    //Test
    public void SetTarget()
    {
        var character = _characters.GetValueOrDefault("SunWukong").GetComponent<Entity>();

        var enemy = _enemies[0].gameObject.GetComponent<Entity>();

        character.SetTaget(enemy);
    }

    private void InitStateMachine()
    {
        StateMachine = new StateMachine<BattleState, BattleBaseState>();
        //SetupState, // Init data, spawn character, load inviroment
        //OrderState, // Decide order enemy
        //BeginTurnBase, // Handle effect or buff
        //ActionState, // Player Chosce SkillCharacter or AI controller
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
