using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DialogueManager : MonoBehaviour
{
    //[Inject] GameStateManager gameState;
    [Inject] GameNarrativeData gameNarrativeData;

    private int counterDialogue;
    private int counterLine;

    private bool reachedEndOfDialogue { get => counterDialogue >= currentDialogue.Lines.Count; }
    private bool reachedEndOfLine { get=> counterLine >= currentDialogue.Lines[counterDialogue].Texts.Count; }
    private DialogueConfig currentDialogue = default;
    private void Awake()
    {
        GameEvent.OnStartDialogue += DisplayDialogueConfig;

    }
    private void Destroy()
    {
        GameEvent.OnStartDialogue -= DisplayDialogueConfig;

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void DisplayDialogueConfig(DialogueConfig dialogueData)
    {
        //if(gameState.CurrentGameState != GameState.Cutscene) // the dialgue state is implied in the cutscene state
        //{
        //    gameState.UpdateGameState(GameState.Dialogue);
        //}
        GameEvent.OnAdvanceDialogueEvent += OnAdvance;

        counterDialogue = 0;
        counterLine = 0;
        currentDialogue = dialogueData;

        if(currentDialogue.Lines != null)
        {
            ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[counterLine].ActorID);
            DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
        }
        else
        {
            Debug.LogError("Check Dialogue");
        }
    }

    public void DisplayDialogueLine(string dialogueLine, ActorConfig actor)
    {
        GameEvent.OnOpenDialogue?.Invoke(dialogueLine, actor);
    }

    private void OnAdvance()
    {
        counterLine++;
        if (!reachedEndOfLine)
        {
            ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[counterDialogue].ActorID);
            DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
        }
        else if (currentDialogue.Lines[counterDialogue].Chocies != null 
            & currentDialogue.Lines[counterDialogue].Chocies.Count > 0)
        {
            // Display Choice
            DisplayChoices(currentDialogue.Lines[counterDialogue].Chocies);
        }
        else
        {
            counterDialogue++;
            if(!reachedEndOfDialogue)
            {
                counterLine = 0;
                ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[counterDialogue].ActorID);
                DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
            }
            else
            {
                // Dialogue end and close DialogueUI
                DialogueEndAndCloseDialogueUI();
                counterLine = 0;
            }
        }
    }

    private void DisplayChoices(List<ChoiceCompement> choices)
    {
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
        GameEvent.OnMakeChocieUI += MakeDialogueChoice;
        GameEvent.OnShowChoiceUI?.Invoke(choices);
    }

    private void MakeDialogueChoice(ChoiceCompement choice)
    {
        GameEvent.OnMakeChocieUI -= MakeDialogueChoice;
        switch (choice.ActionType)
        {
            case ChoiceActionType.DoNothing:
                if (choice.NextDialogue != null)
                {
                    DialogueConfig nextDialogue = gameNarrativeData.GetDialogueConfigByID(choice.NextDialogue);
                    DisplayDialogueConfig(nextDialogue);
                }
                else
                    DialogueEndAndCloseDialogueUI();
                break;
            case ChoiceActionType.ContinueWithStep:
                {

                }
                break;
            case ChoiceActionType.WinningChoice:
                {
                    GameEvent.OnMakeWinChoice?.Invoke();
                }
                break;
            case ChoiceActionType.LosingChoice:
                {
                    GameEvent.OnMakeLosingChoice?.Invoke();
                }
                break;
            case ChoiceActionType.IncompleteStep:
                {

                }
                break;
        }
    }

    private void DialogueEndAndCloseDialogueUI()
    {
        // raise the special event for end of dialogue if any
        //currentDialogue.FinishDialogue();
        // raise end dialogue event

        GameEvent.OnEndDialogue?.Invoke(currentDialogue.Type);
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;

        // gameState reset state
    }
}
