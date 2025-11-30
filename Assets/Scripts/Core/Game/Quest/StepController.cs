using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class StepController : MonoBehaviour
{
    [SerializeField] private ActorSO actor = default;

    // Default Dialogue
    // Quest Data
    //[Inject] private GameStateManager gameState;
    /// <summary>
    ///
    /// </summary>
    //Event
    // Win Dialogue Event
    // Lose Dialogue Evemt
    // End Dialogue Event

    // Start Dialogue Event

    // private DialogueData currentDialogue;

    [Inject] GameNarrativeData gameNarrativeData;
    [Inject] QuestManager questManager;
    private DialogueData defaultDialogue = new();

    private DialogueData currentDialogue;
    void Start()
    {
        //resolver.Inject(this);
        defaultDialogue = gameNarrativeData.GetDefaultDialogueDataByActorID(actor.ID);
    }

    void PlayDefaultDialogue()
    {
        if(defaultDialogue != null)
        {
            currentDialogue = defaultDialogue;
            StartDialogue();
        }
    }
    // start a dialgoue when interation
    // some Steps need to be instantanios. And do not need the interact button
    // when interatoin again. restart same dialogue
    public void InteractWithCharacter()
    {
        DialogueData displayDialogue = questManager.InteractWithCharacter(actor.ID, false, false);

        if(displayDialogue != null)
        {
            currentDialogue = displayDialogue;
            StartDialogue();
            Debug.Log("QuestLine Dialogue");
        }
        else
        {
            PlayDefaultDialogue();
            Debug.Log("Default Dialogue");
        }
    }

    private void StartDialogue()
    {
        GameEvent.OnStartDialogue(currentDialogue);
        GameEvent.OnEndDialogue += EndDialogue;

        GameEvent.OnWinDialogue += PlayWinDialogue;
        GameEvent.OnLoseDialogue += PlayLoseDialogue;
    }

    private void EndDialogue(DialogueType type)
    {
        GameEvent.OnEndDialogue -= EndDialogue;

        GameEvent.OnWinDialogue -= PlayWinDialogue;
        GameEvent.OnLoseDialogue -= PlayLoseDialogue;

    }

    void PlayLoseDialogue()
    {
        if(questManager != null)
        {
            DialogueData displayDialogue = questManager.InteractWithCharacter(actor.ID, true, false);
            if(displayDialogue != null)
            {
                currentDialogue = displayDialogue;
                StartDialogue();
            }
        }
    }

    void PlayWinDialogue()
    {
        DialogueData displayDialogue = questManager.InteractWithCharacter(actor.ID, true, true);
        if (displayDialogue != null)
        {
            currentDialogue = displayDialogue;
            StartDialogue();
        }
    }
}
