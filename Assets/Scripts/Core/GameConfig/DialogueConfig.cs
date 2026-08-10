using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum ChoiceActionType
{
    DoNothing = 0,
    NextNode = 1,          // Sang node hội thoại kế tiếp
    AcceptQuest = 2,       // Nhận Quest
    CompleteStep = 3,      // Hoàn thành bước nhiệm vụ
    GiveItem = 4,          // Giao vật phẩm
    OpenShop = 5,          // Mở cửa hàng
    CloseDialogue = 6,     // Đóng UI hội thoại
    ContinueWithStep = 7,  // Tương thích ngược
    WinningChoice = 8,     // Tương thích ngược
    LosingChoice = 9,      // Tương thích ngược
    IncompleteStep = 10,   // Tương thích ngược
    Reject = 11,           // Từ chối Quest
    RejectQuest = 11       // Alias cho Reject
}

public enum DialogueType
{
    Start,
    Completion,
    Incompletion,
    Default,
    Normal
}

[Serializable]
public class DialogueChoiceConfig
{
    [JsonProperty("text_hash")]
    public long TextHash;

    [JsonProperty("action_type")]
    public ChoiceActionType ActionType;

    [JsonProperty("target_node_id")]
    public string TargetNodeID;

    [JsonProperty("param")]
    public string Param;
}

[Serializable]
public class DialogueNodeConfig
{
    [JsonProperty("node_id")]
    public string NodeID;

    [JsonProperty("actor_id")]
    public string ActorID;

    [JsonProperty("text_hash")]
    public long TextHash;

    [JsonProperty("next_node_id")]
    public string NextNodeID;

    [JsonProperty("step_end")]
    public bool StepEnd;

    [JsonProperty("is_step_end")]
    public bool IsStepEnd;

    [JsonProperty("choices")]
    public List<DialogueChoiceConfig> Choices = new();

    [JsonIgnore]
    public string Text;

    [JsonIgnore]
    public List<string> Texts = new();
}

[Serializable]
public class ChoiceComponent
{
    [JsonProperty("text_hash")]
    public long Text;

    [JsonProperty("type")]
    public ChoiceActionType ActionType;

    [JsonProperty("next_dialogue")]
    public string NextDialogue;

    [JsonProperty("target_node_id")]
    public string TargetNodeID;

    [JsonProperty("param")]
    public string Param;
}

[Serializable]
public class LineComponent
{
    [JsonProperty("text_hash")]
    public long Text;

    [JsonProperty("actor_id")]
    public string ActorID;

    [JsonProperty("choices")]
    public List<ChoiceComponent> Choices;

    [JsonIgnore]
    public List<string> Texts;
}

[Serializable]
public class DialogueConfig
{
    [JsonProperty("id")]
    public string ID;

    [JsonProperty("type")]
    public DialogueType Type;

    [JsonProperty("lines")]
    public List<LineComponent> Lines;

    [JsonProperty("nodes")]
    public List<DialogueNodeConfig> Nodes = new();
}


