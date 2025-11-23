using System;
public class LineConfig 
{
    private string id;
    private string dialogueID;
    private string actorID;
    private string texts;
    private bool hasChoice;

    public string ID { get { return id; } set { id = value; } }
    public string DialogueID { get { return dialogueID; } set { dialogueID = value; } }
    public string ActorID { get { return actorID; } set { actorID = value; } }
    public string Texts { get { return texts; } set { texts = value; } }
    public bool HasChoice { get { return hasChoice; } set { hasChoice = value; } }
}
