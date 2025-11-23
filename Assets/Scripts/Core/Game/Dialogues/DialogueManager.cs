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
    private DialogueData currentDialogue = default;
    private void OnEnable()
    {
        GameEvent.OnStartDialogue += DisplayDialogueData;
    }
    private void OnDisable()
    {
        GameEvent.OnStartDialogue -= DisplayDialogueData;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void DisplayDialogueData(DialogueData dialogueData)
    {
        //if(gameState.CurrentGameState != GameState.Cutscene) // the dialgue state is implied in the cutscene state
        //{
        //    gameState.UpdateGameState(GameState.Dialogue);
        //}

        counterDialogue = 0;
        counterLine = 0;
        currentDialogue = dialogueData;

        if(currentDialogue.Lines != null)
        {
            ActorData actor = gameNarrativeData.GetActorData(currentDialogue.Lines[counterLine].ActorID);
            DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
        }
        else
        {
            Debug.LogError("Check Dialogue");
        }
    }

    public void DisplayDialogueLine(string dialogueLine, ActorData actor)
    {
        GameEvent.OnOpenDialogue?.Invoke(dialogueLine, actor);
    }
}
