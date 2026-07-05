using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public enum StepType
{
    Dialogue,
    GiveItem,
    CheckItem
}

public enum QuestType
{
    None = 0,
    Main,
    Daily
}

[Serializable]
public class StepCompoment
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("actor_id")]
    public string ActorID;

    [JsonProperty("previous_diagoue")]
    public string PreviousDialogue;

    [JsonProperty("completed_dialogue")]
    public string CompletedDialogue;

    [JsonProperty("incompleted_dialogue")]
    public string IncompleteDialogue;

    [JsonProperty("type")]
    public StepType Type;

    [JsonProperty("item_id")]
    public string ItemID;
}

[Serializable]
public class QuestCompoment
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("steps")]
    public List<StepCompoment> Steps;

    [JsonProperty("quest_type")]
    public QuestType Type;

    [JsonProperty("reward_id")]
    public string RewardID;
}

[Serializable]
public class QuestLineConfig
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("quests")]
    public List<QuestCompoment> Quests;
}




