using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public enum StepType
{
    Dialogue,
    GiveItem,
    CheckItem
}

[Serializable]
public class StepCompoment
{
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

    [JsonProperty("has_reward")]
    public bool HasReward;

    [JsonProperty("reward_id")]
    public string RewardID;

    [JsonIgnore]
    public bool IsDone = false;
}

[Serializable]
public class QuestCompoment
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("steps")]
    public List<StepCompoment> Steps;

    [JsonIgnore]
    public bool IsDone = false;
}

[Serializable]
public class QuestLineConfig
{
    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("quests")]
    public List<QuestCompoment> Quests;

    [JsonIgnore]
    public bool IsDone = false;
}




