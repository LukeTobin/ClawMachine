using System;
using System.Collections.Generic;

public static class EventManager
{
    private static Dictionary<Event, Delegate> eventDictionary = new Dictionary<Event, Delegate>();

    public enum Event
    {
        OnClawStandby,
        OnControlButtonClick,
        OnClawControllable,
        OnClawReturning
    }
    
    public static void Subscribe(Event eventType, Action listener)
    {
        eventDictionary.TryAdd(eventType, null);
        eventDictionary[eventType] = Delegate.Combine(eventDictionary[eventType], listener);
    }
    
    public static void Unsubscribe(Event eventType, Action listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] = Delegate.Remove(eventDictionary[eventType], listener);
        }
    }

    public static void Invoke(Event eventType)
    {
        if (eventDictionary.TryGetValue(eventType, out Delegate eventDelegate))
        {
            (eventDelegate as Action)?.Invoke();
        }
    }
}
