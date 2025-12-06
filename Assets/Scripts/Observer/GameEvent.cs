using System;
using UnityEngine;
using System.Collections.Generic;


public static class GameEvent
{
    //Scene Load
    public static Action<GameSceneSO,bool, bool> OnLoadColdStartupLocation;
    public static Action<GameSceneSO, bool, bool> OnLoadSceneLocation;

    //Gamestart
    public static Action OnStartNewGame;


    public static Action<Vector2> OnPlayerMove;
    public static Action<Vector2> OnCameraMove;

    public static Action OnPlayerAttack;
    public static Action OnPlayerTransform;

    // Dialgoue
    public static Action<DialogueData> OnStartDialogue;
    public static Action<string, ActorData> OnOpenDialogue;
    public static Action<DialogueType> OnEndDialogue;
    public static Action OnAdvanceDialogueEvent;
    public static Action<List<ChoiceData>> OnShowChoiceUI;
    public static Action<ChoiceData> OnMakeChocieUI;
    public static Action OnWinDialogue;
    public static Action OnLoseDialogue;

    // Quest
    public static Action OnMakeWinChoice;
    public static Action OnMakeLosingChoice;
    public static Action OnContinueWithStepEvent;


    public static Action OnInteraction;
}
