using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Signal : MonoBehaviour {
    private static Dictionary<string, Action<object>> eventHandlers = new Dictionary<string, Action<object>>();

    public static void Subscribe(string eventName, Action<object> listener)
    {
        if (!eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] = null;
        }

        eventHandlers[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] -= listener;
        }
    }

    public static void Emit(string eventName, object data)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName]?.Invoke(data);
        }
    }

    public static void Clear(string eventName)
    {
        if (eventHandlers.ContainsKey(eventName))
        {
            eventHandlers[eventName] = null;
        }
    }

    public static void ClearAll()
    {
        eventHandlers.Clear();
    }
}
