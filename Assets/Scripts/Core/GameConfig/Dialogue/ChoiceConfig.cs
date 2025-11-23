using System;
public class ChoiceConfig 
{
    private string id;
    private string lineID;
    private ChoiceActionType actionType;
    private string text;
    private string nextDialogueID;

    public string ID { get { return id; }  set { id = value; } }
    public string LineID { get { return lineID; } set { lineID = value; } }
    public ChoiceActionType ActionType { get { return actionType; } set { actionType = value; } }
    public string Text { get { return text; } set { text = value; } }   
    public string NextDialogueID { get { return nextDialogueID; } set { nextDialogueID = value; } }
}
