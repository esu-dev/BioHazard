using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerProxy : MonoBehaviour
{
    [SerializeField]
    EventTrigger _eventTrigger;

    public void AddEvent(EventTriggerType eventTriggerType, Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener(data => action());
        _eventTrigger.triggers.Add(entry);
    }
}
