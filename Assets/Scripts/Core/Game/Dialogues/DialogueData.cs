using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    Default
}
public class DialogueData
{
    private string id;
    private DialogueType type;
    private List<LineData> lines;
    private string actorID;

    public List<LineData> Lines => lines;
    public string ActorID => actorID;   
    public string ID => id;

    public void InitData(string id, DialogueType type, List<LineData> lines, string actorID)
    {
        this.id = id;
        this.type = type;
        this.lines = lines;
        this.actorID = actorID;
    }
}

public class ChoiceData
{
    private string id;
    private string lineID;
    private string response;
    private ChoiceActionType actionType;
    private string nextDialogueID;

    public string LineID => lineID;

    public void InitData(string id,string lineID, string response, ChoiceActionType actionType, string nextDialogueID)
    {
        this.id = id;
        this.lineID = lineID;
        this.response = response;
        this.actionType = actionType;
        this.response = nextDialogueID;
    }
}

public class LineData
{
    private string id;
    private string dialogueID;
    private string actorID;
    private List<string> texts;
    private ChoiceData choiceID;

    public string DialogueID => dialogueID;
    public List<string> Texts => texts;
    public string ActorID => actorID;   
    public void InitData(string id, string dialogueID, string actorID, List<string> texts, ChoiceData choiceID)
    {
        this.id = id;
        this.dialogueID = dialogueID;
        this.actorID = actorID;
        this.texts = texts;
        this.choiceID = choiceID;
    }
}
