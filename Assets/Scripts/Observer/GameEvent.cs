using System;
using UnityEngine;
using System.Collections.Generic;


public static class GameEvent
{
    //Scene Load
    public static Action<GameSceneSO,bool, bool> OnLoadColdStartupLocation;
    public static Action<GameSceneSO, bool, bool> OnLoadSceneLocation;

    public static Action OnSceneReady;
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


    public static Action OnInteraction;
}
