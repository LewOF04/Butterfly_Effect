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
    public bool wasPositivePerf; //was the action positive for the performer
    public bool wasPositiveRec; //was the action positive for the receiver
    public float performerImportance; //how important the event is in the performing NPCs mind
    public float receiverImportance; //how important the event is in the receiving NPCs mind

    public NPCEvent(NPCEventKey key, string name, string desc, float sev, float time, int perf, int rec, bool wasPP, bool wasPR, float perImp, float recImp)
    {
        eventKey = key;
        actionName = name;
        description = desc;
        severity = sev;
        timeOfAction = time;
        performer = perf;
        receiver = rec;
        wasPositivePerf = wasPP;
        wasPositiveRec = wasPR;
        performerImportance = perImp;
        receiverImportance = recImp;

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