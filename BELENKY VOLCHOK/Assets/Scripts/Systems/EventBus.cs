using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<string, Action> noParamEvents = new Dictionary<string, Action>();
    private static Dictionary<string, Action<object>> singleParamEvents = new Dictionary<string, Action<object>>();
    
    public static void Listen(string eventName, Action listener)
    {
        if (!noParamEvents.ContainsKey(eventName))
            noParamEvents[eventName] = null;
        noParamEvents[eventName] += listener;
    }
    
    public static void Listen(string eventName, Action<object> listener)
    {
        if (!singleParamEvents.ContainsKey(eventName))
            singleParamEvents[eventName] = null;
        singleParamEvents[eventName] += listener;
    }
    
    public static void StopListening(string eventName, Action listener)
    {
        if (noParamEvents.ContainsKey(eventName))
            noParamEvents[eventName] -= listener;
    }
    
    public static void StopListening(string eventName, Action<object> listener)
    {
        if (singleParamEvents.ContainsKey(eventName))
            singleParamEvents[eventName] -= listener;
    }
    
    public static void Broadcast(string eventName)
    {
        if (noParamEvents.ContainsKey(eventName))
            noParamEvents[eventName]?.Invoke();
    }
    
    public static void Broadcast(string eventName, object data)
    {
        if (singleParamEvents.ContainsKey(eventName))
            singleParamEvents[eventName]?.Invoke(data);
    }
}
