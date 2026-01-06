using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class QuestManager 
{
    private Dictionary<string, QuestLineConfig> questLines = new();

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

    private QuestLineConfig currentQuestLine;
    private QuestCompoment currentQuest;
    private StepCompoment currentStep;
    private string currentQuestLineIndex;
    private int currentQuestIndex;
    private int currentStepIndex;

    // function
    // void StartQuestLine()
    //bool HasStep(ActorConfig actorToCheckWith)

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
        // Check from Player Profile

        // exam
        //questLines = gameNarrativeData.QuestLineConfigs;
        //if(questLines != null)
        //{
        //    if(questLines.Exists(o => !o.IsDone))
        //    {
        //        currentQuestLineIndex =  questLines.FindIndex(o => !o.IsDone);
        //        if(currentQuestLineIndex >= 0)
        //        {
        //            currentQuestLine = questLines.Find(o => !o.IsDone);
        //        }
        //    }
        //}

        questLines = gameNarrativeData.QuestLineConfigs;

        if (questLines != null)
        {
            // Tìm QuestLine đầu tiên chưa hoàn thành (IsDone == false)
            // Sử dụng LINQ FirstOrDefault để lấy ra object thỏa mãn điều kiện
            var firstUnfinished = questLines.FirstOrDefault(kvp => !kvp.Value.IsDone);

            // Kiểm tra xem có tìm thấy bản ghi nào không (nếu Key là string, null nghĩa là không thấy)
            if (firstUnfinished.Key != null)
            {
                currentQuestLine = firstUnfinished.Value;
                currentQuestLineIndex = firstUnfinished.Key; // Trong Dictionary, Index thường chính là Key (ID)
            }
            else
            {
                // Xử lý khi tất cả Quest đã hoàn thành hoặc Dictionary trống
                currentQuestLine = null;
                currentQuestLineIndex = null;
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
    public DialogueConfig InteractWithCharacter(string actor, bool isCheckValidity, bool isValid)
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
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue);
                }
                else
                {
                    return gameNarrativeData.GetDialogueConfigByID(currentStep.IncompleteDialogue);
                }
            }
            else
            {
                return gameNarrativeData.GetDialogueConfigByID(currentStep.PreviousDialogue);
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
                        if(gameNarrativeData.GetDialogueConfigByID(currentStep.CompletedDialogue) != null)
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

            var questline = questLines.FirstOrDefault(kvp => !kvp.Value.IsDone);

            if (questline.Value != null)
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
                if (currentStep.HasReward && currentStep.RewardID != null)
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
