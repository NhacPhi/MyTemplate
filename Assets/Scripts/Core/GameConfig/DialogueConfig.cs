using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum ChoiceActionType
{
    DoNothing,
    ContinueWithStep,
    WinningChoice,
    LosingChoice,
    IncompleteStep
}

public enum DialogueType
{
    Start,
    Completetion,
    Incompletion,
    Default,
    Normal
}

[Serializable]
public class ChoiceCompement
{
    [JsonProperty("text_hash")]
    public long Text;

    [JsonProperty("type")]
    public ChoiceActionType ActionType;

    [JsonProperty("next_dialogue")]
    public string NextDialogue;
}

[Serializable]
public class LineCompement
{
    [JsonProperty("text_hash")]
    public long Text;

    [JsonProperty("actor_id")]
    public string ActorID;

    [JsonProperty("choices")]
    public List<ChoiceCompement> Chocies;

    [JsonIgnore]
    public List<string> Texts;
}

[Serializable]
public class DialogueConfig
{
    [JsonProperty("type")]
    public DialogueType Type;

    [JsonProperty("lines")]
    public List<LineCompement> Lines;
}


