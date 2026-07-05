using System;
using UnityEngine;
using System.Collections.Generic;


public static class GameEvent
{
    //Scene Load
    public static Action<GameSceneSO,bool, bool> OnLoadColdStartupLocation;
    public static Action<GameSceneSO, bool, bool> OnLoadSceneLocation;

    public static Action OnSceneReady;
    public static bool IsSceneReady;
    public static Action OnPlayerSpawned;

    //Gamestart
    public static Action OnStartNewGame;


    public static Action<Vector2> OnPlayerMove;
    public static Action<Vector2> OnCameraMove;

    public static Action OnPlayerAttack;
    public static Action OnPlayerTransform;

    // Dialgoue
    public static Action<DialogueConfig> OnStartDialogue;
    public static Action<string, ActorConfig> OnOpenDialogue;
    public static Action<DialogueType> OnEndDialogue;
    public static Action OnAdvanceDialogueEvent;
    public static Action<List<ChoiceCompement>> OnShowChoiceUI;
    public static Action<ChoiceCompement> OnMakeChocieUI;
    public static Action OnWinDialogue;
    public static Action OnLoseDialogue;

    // Quest
    public static Action OnMakeWinChoice;
    public static Action OnMakeLosingChoice;
    public static Action OnContinueWithStepEvent;
    public static Action OnQuestUpdated;
    public static Action<QuestCompoment> OnOpenQuestUI;
    public static Action<StepController> OnNPCSpawned;
    public static Action<StepController> OnNPCDestroyed;


    public static Action OnInteraction;
    public static Action<Interaction> OnExecuteSpecificInteraction;
    public static Action<string, int> OnRequestPickupItem;

    // Respawn System
    public static Func<string, RespawnType, float, bool> OnCheckRespawnStatus;
    public static Action<string> OnRecordResourceDestroyed;

    // Daily Quest Tracking
    public static Action<string, int> OnEnemyKilled; 
    public static Action<string, int> OnPickupItem; 
    public static Action<string, int> OnCharacterUpgraded; 
    public static Action<int> OnWeaponUpgraded; 
    public static Action<string, int> OnWinBattle; 
    public static Action<string, int> OnShopPurchased;
    public static Action<string, int> OnGachaSummoned;
    public static Action OnDailyQuestUpdated;
}
