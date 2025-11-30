using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class QuestManager 
{
    private List<QuestLineData> questLines = new();

    //event listen
    // continueWithStepEvent
    // endDialogueEvent
    // makeWinningChoiceEvent
    // makeLosingChocieEvent

    // event boardcasting
    // playCompletionDialogueEvent
    //  playIncompleteDialgoueEvent
    // startLosingCutscene
    // giveItemEvent
    // rewardItemEven

    private QuestLineData currentQuestLine;
    private QuestData currentQuest;
    private StepData currentStep;
    private int currentQuestLineIndex;
    private int currentQuestIndex;
    private int currentStepIndex;

    // function
    // void StartQuestLine()
    //bool HasStep(ActorData actorToCheckWith)

    [Inject] GameNarrativeData gameNarrativeData;

    
    public void StartGame()
    {
        GameEvent.OnMakeWinChoice += MakeWinningChoice;
        GameEvent.OnMakeWinChoice -= MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent += CheckStepValidity;

        GameEvent.OnEndDialogue += EndDialogue;  // Check end of Diagloe has reward
        StartQuestLine();
    }

    ~QuestManager() {
        GameEvent.OnMakeWinChoice -= MakeWinningChoice;
        GameEvent.OnMakeWinChoice -= MakeWinningChoice;
        GameEvent.OnContinueWithStepEvent -= CheckStepValidity;
        GameEvent.OnEndDialogue -= EndDialogue;
    }
    void StartQuestLine()
    {
        questLines = gameNarrativeData.QuestLines;
        if(questLines != null)
        {
            if(questLines.Exists(o => !o.IsDone))
            {
                currentQuestLineIndex =  questLines.FindIndex(o => !o.IsDone);
                if(currentQuestLineIndex >= 0)
                {
                    currentQuestLine = questLines.Find(o => !o.IsDone);
                }
            }
        }
    }

    bool CheckQuestLineForQuestWithActor(string actorToCheckWith)
    {
        if (currentQuest == null)//check if there's a current quest 
        {
            if (currentQuestLine != null)
            {

                return currentQuestLine.Quests.Exists(o => !o.IsDone && o.Steps != null && o.Steps[0].ActorID == actorToCheckWith);

            }

        }
        return false;
    }

    bool HasStep(string actorToCheckWith)
    {
        if(currentQuest != null)
        {
            if(currentStep.ActorID == actorToCheckWith)
            {
                return true;
            }
        }

        return false;
    }
    public DialogueData InteractWithCharacter(string actor, bool isCheckValidity, bool isValid)
    {
        if(currentQuest == null)
        {
            if(CheckQuestLineForQuestWithActor(actor))
            {
                StartQuest(actor);
            }
        }

        if(HasStep(actor))
        {
            if(isCheckValidity)
            {
                if(isValid)
                {
                    return currentStep.CompleteDialogue;
                }
                else
                {
                    return currentStep.IncompleteDialogue;
                }
            }
            else
            {
                return currentStep.DialogueBeforeStep;
            }
        }
        return null;
    }
    // When Interacting with character, we ask the quest manager if there's a quest that start a step with a certain character
    void StartQuest(string actorToCheckWith)
    {
        if(currentQuest != null) //check if there's a current quest 
        {
            return;
        }

        if(currentQuestLine != null)
        {
            // find quest index
            currentQuestIndex = currentQuestLine.Quests.FindIndex(o => !o.IsDone && o.Steps != null &&
            o.Steps[0].ActorID == actorToCheckWith);

            if((currentQuestLine.Quests.Count > currentQuestIndex) && (currentQuestIndex >= 0))
            {
                currentQuest = currentQuestLine.Quests[currentQuestIndex];
                // start Step
                currentStepIndex = 0;
                currentStepIndex = currentQuest.Steps.FindIndex(o => o.IsDone == false);
                if(currentStepIndex >= 0)
                {
                    StartStep();
                }
            }
        }
    }

    void MakeWinningChoice()
    {
        // Call EndStepEvent
        CheckStepValidity();
    }
    void MakLosingChoice()
    {
        CheckStepValidity();
    }
    void StartStep()
    {
        if(currentQuest.Steps != null)
        {
            if(currentQuest.Steps.Count > currentStepIndex)
            {
                currentStep = currentQuest.Steps[currentStepIndex];
            }
        }
    }
    void CheckStepValidity()
    {
        if(currentStep != null)
        {
            switch(currentStep.Type)
            {
                case StepType.CheckItem:
                    {
                        // check item in Inventory
                        // Call event CompleteDialogue or IncompleteDialogue
                    }
                    break;
                case StepType.GiveItem:
                    {
                        // check item in Inventory
                        // Call event GiveItem(currentStep.Item) or IncompleteDialogue
                    }
                    break;
                case StepType.Dialogue:
                    {
                        if(currentStep.CompleteDialogue != null)
                        {
                            // Call CompleteDialogue
                        }
                        else
                        {
                            EndStep();
                        }
                    }
                    break;
            }
        }
    }

    void EndStep()
    {
        currentStep = null;
        if (currentQuest != null)
            if (currentQuest.Steps.Count > currentStepIndex)
            {
                //currentQuest.Steps[currentStepIndex].FinishStep();
                //saveSystem.SaveDataToDisk();
                if (currentQuest.Steps.Count > currentStepIndex + 1)
                {
                    currentStepIndex++;
                    StartStep();

                }
                else
                {

                    EndQuest();
                }
            }
    }

    void EndQuest()
    {

        if (currentQuest != null)
        {
            //currentQuest.FinishQuest();
            //saveSystem.SaveDataToDisk();
        }
        currentQuest = null;
        currentQuestIndex = -1;
        if (currentQuestLine != null)
        {
            if (currentQuestLine.Quests.Exists(o => !o.IsDone))
            {
                EndQuestline();

            }

        }
    }

    void EndQuestline()
    {
        if (questLines != null)
        {
            if (currentQuestLine != null)
            {
                //currentQuestLine.FinishQuestline();
                //saveSystem.SaveDataToDisk();

            }

            if (questLines.Exists(o => o.IsDone))
            {
                StartQuestLine();

            }

        }


    }

    void EndDialogue(DialogueType dialogueType)
    {

        //depending on the dialogue that ended, do something 
        switch (dialogueType)
        {
            case DialogueType.Completetion:
                if (currentStep.HasReward && currentStep.Reward != null)
                {
                    //ItemStack itemStack = new ItemStack(_currentStep.RewardItem, _currentStep.RewardItemCount);
                    //_rewardItemEvent.RaiseEvent(itemStack);
                }
                EndStep();
                break;
            case DialogueType.Start:
                CheckStepValidity();
                break;
            default:
                break;
        }
    }
    // EndDialogue
    // EndStep()
    // EndQuest()
    // EndQuestLine()
    // GetFinishedQuestlineItemsGUIds()
    // SetFinishedQuestlineItemsFromSave()
    // ResetQuestlines()
    // IsNewGame()
}
