using System.Collections;
using System.Collections.Generic;
using System;

public class QuestLineData 
{
    private string id;
    private List<QuestData> quests;
    private bool isDone;
    private string endQuestLineEventID;

    public string ID => id;
    public List<QuestData> Quests => quests;
    public bool IsDone => isDone;
    public string EndQuestLineEventID => endQuestLineEventID;
    public void InitData(string id, List<QuestData> quests, string endQuestLineEventID)
    {
        this.id = id;
        this.quests = quests;
        this.endQuestLineEventID = endQuestLineEventID;
    }
}

public class QuestData
{
    private string id;
    private string questLineID;
    private List<StepData> steps;
    private bool isDone = false;
    private string endQuestEventID;

    public string ID => id;
    public string QuestLineID => questLineID;
    public List<StepData> Steps => steps;
    public bool IsDone => isDone;  
    public string EndQuestEventID => endQuestEventID;
    public void InitData(string id, string questLineID, List<StepData> steps, string endQuestEventID)
    {
        this.id = id;
        this.questLineID = questLineID;
        this.steps = steps;
        this.endQuestEventID = endQuestEventID;
    }
}

public class StepData
{ 
    private string id;
    private string questID;
    private string actorID;
    private DialogueData dialogueBeforeStep;
    private DialogueData completeDialogue;
    private DialogueData incompleDialogue;
    private StepType type;
    private string itemID;
    private bool hasReward;
    private RewardPayLoad reward;
    private bool isDone = false;
    private string endStepEventID;
    public string ID => id;
    public string QuestID => questID;
    public string ActorID => actorID;
    public DialogueData DialogueBeforeStep => dialogueBeforeStep;
    public DialogueData CompleteDialogue => completeDialogue;
    public DialogueData IncompleteDialogue => incompleDialogue;
    public StepType Type => type;
    public string ItemID => itemID;
    public bool HasReward => hasReward;
    public RewardPayLoad Reward => reward;
    public bool IsDone => isDone;
    public string EndStepEventID => endStepEventID;

    public void InitData(string id, string questID, string actorID, DialogueData dialogueBeforeStep, DialogueData completeDialogue,
       DialogueData incompleDialogue,StepType type, string itemID, bool hasReward, RewardPayLoad reward, string endStepEventID)
    {
        this.id = id;
        this.questID = questID;
        this.actorID = actorID;
        this.dialogueBeforeStep = dialogueBeforeStep;
        this.completeDialogue = completeDialogue;
        this.incompleDialogue = incompleDialogue;
        this.type = type;
        this.itemID = itemID;
        this.hasReward = hasReward;
        this.reward = reward;
        this.endStepEventID = endStepEventID;
    }
}


