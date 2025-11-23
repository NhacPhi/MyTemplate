using System;

public class DialogueConfig
{
    private string id;
    private DialogueType type;
    private string actorID;
    public string ID { get { return id; } set { id = value; } }
    public DialogueType Type { get { return type;} set { type = value; } }

    public string ActorID { get { return actorID; } set { actorID = value; } }
}
