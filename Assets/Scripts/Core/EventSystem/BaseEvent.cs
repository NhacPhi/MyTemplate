using System;

public class BaseEvent
{
    private string eventID;
    private object payload;

    public string EventID { get { return eventID; } set { eventID = value; } }
    public object PayLoad { get { return payload; } set { payload = value; } }
}
