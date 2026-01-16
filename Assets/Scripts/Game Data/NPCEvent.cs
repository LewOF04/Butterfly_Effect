using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCEvent
{
    public NPCEventKey eventKey; //unique key identifier of the key
    public string actionName; //the name of the action that took place
    public string description; //description of the historical event
    public float severity; //how importance was this event to the npc
    public float timeOfAction; //when this action occurred
    public int performer; //the npc that performed the action
    public int receiver; //the npc that received the action
    public bool wasPositive; //was the action positive for the performer
    public bool wasSuccessful; //was the action positive for the performer
    public float importance;

    public NPCEvent(NPCEventKey key, string name, string desc, float sev, float time, int perf, int rec, bool wasP, bool wasS, float imp)
    {
        eventKey = key;
        actionName = name;
        description = desc;
        severity = sev;
        timeOfAction = time;
        performer = perf;
        receiver = rec;
        wasPositive = wasP;
        wasSuccessful = wasS;
        importance = imp;
    }
}

/*
Wrapper for NPC History Item list that can be used as a stepping stone when serializing and de-serializing
*/
[System.Serializable]
public class NPCEventDatabase
{
    public List<NPCEvent> items = new List<NPCEvent>();
}