using System;
public class QuestLineConfig
{
    private string id;
    private string name;
    private string description;
    private string eventID;

    public string ID { get { return id; } set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Description { get { return description; } set { description = value; } }
    public string EventID { get { return eventID; } set { eventID = value; } }
}

public class QuestConfig
{
    private string id;
    private string questLineID;
    private string name;
    private string description;
    private string eventID;


    public string ID { get { return id; } set { id = value; } }
    public string QuestLineID { get { return questLineID; } set { questLineID = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Description { get { return description; } set { description = value; } }
    public string EventID { get { return eventID; } set { eventID = value; } }
}

public enum StepType
{
    Dialogue,
    GiveItem,
    CheckItem
}
public class StepConfig
{
    private string id;
    private string questID;
    private string actorID;
    private string dialogueBeforeStep;
    private string completeDialogue;
    private string incompleteDialogue;
    private StepType type;
    private string itemID;
    private bool hasReward;
    private string rewardID;
    private string eventID;

    public string ID { get { return id; } set { id = value; } }
    public string QuestID { get { return questID; } set { questID = value; } }
    public string ActorID { get { return actorID; } set { actorID = value; } }
    public string DialogueBeforeStep { get { return dialogueBeforeStep; } set { dialogueBeforeStep = value; } }
    public string CompleteDialogue { get { return completeDialogue; } set { completeDialogue = value; } }
    public string IncompleteDialogue { get { return incompleteDialogue; } set { incompleteDialogue = value; } }
    public StepType Type { get { return type;} set { type = value; } }
    public string ItemID { get { return itemID; } set { itemID = value; } }
    public bool HasReward { get { return hasReward; } set { hasReward = value; } }
    public string RewardID { get { return rewardID; } set { rewardID = value;} }
    public string EventID { get { return eventID; } set { eventID = value; } }
}