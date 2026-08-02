using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public enum QuestCategory
{
    Main = 0,      // Nhiệm vụ chính tuyến
    Side = 1,      // Nhiệm vụ phụ
    Daily = 2      // Nhiệm vụ hàng ngày
}

public enum StepType
{
    Dialogue,
    GiveItem,
    CheckItem,
    TalkToNPC,
    DefeatEnemy,
    CollectItem,
    ReachLocation,
    InteractObject
}

public enum QuestType
{
    None = 0,
    Main,
    Daily
}

[Serializable]
public class StepComponent
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("actor_id")]
    public string ActorID;

    [JsonProperty("previous_dialogue")]
    public string PreviousDialogue;

    [JsonProperty("completed_dialogue")]
    public string CompletedDialogue;

    [JsonProperty("incomplete_dialogue")]
    public string IncompleteDialogue;

    [JsonProperty("type")]
    public StepType Type;

    [JsonProperty("item_id")]
    public string ItemID;

    [JsonProperty("target_id")]
    public string TargetID;

    [JsonProperty("required_amount")]
    public int RequiredAmount = 1;
}

[Serializable]
public class QuestComponent
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("chapter_id")]
    public string ChapterID;

    [JsonProperty("name_hash")]
    public long Name;

    [JsonProperty("des_hash")]
    public long Description;

    [JsonProperty("prerequisite_quest_ids")]
    public List<string> PrerequisiteQuestIDs = new();

    [JsonProperty("required_level")]
    public int RequiredLevel = 1;

    [JsonProperty("steps")]
    public List<StepComponent> Steps;

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
    public List<QuestComponent> Quests;
}




