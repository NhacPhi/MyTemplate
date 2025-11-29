using System;
using System.Collections.Generic;
using Tech.Singleton;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, Action<BaseEvent>> eventListeners = new();

    public void Register(string eventId, Action<BaseEvent> handler)
    {
        if(!eventListeners.ContainsKey(eventId))
        {
            eventListeners[eventId] = delegate { };
        }
        eventListeners[eventId] += handler;
    }
    
    public void Publish(BaseEvent e)
    {
        if (eventListeners.ContainsKey(e.EventID))
            eventListeners[e.EventID]?.Invoke(e);
    }
}
