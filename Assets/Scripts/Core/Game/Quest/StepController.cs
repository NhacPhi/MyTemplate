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
    // Start is called before the first frame update
    //[Inject] IObjectResolver resolver;
    [Inject] GameNarrativeData gameNarrativeData;
    private DialogueData currentDialogue = new();
    void Start()
    {
        //resolver.Inject(this);
        currentDialogue = gameNarrativeData.GetDialogueDataByActorID(actor.ID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InteractWithCharacter()
    {
        if(currentDialogue != null)
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        GameEvent.OnStartDialogue(currentDialogue);
        GameEvent.OnEndDialogue += EndDialogue;
    }

    private void EndDialogue(DialogueType type)
    {
        GameEvent.OnEndDialogue -= EndDialogue;

    }
}
