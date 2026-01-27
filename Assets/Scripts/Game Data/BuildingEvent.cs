using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingEvent
{
    public BuildingEventKey eventKey; //unique key identifier of the key
    public string actionName; //the name of the action that took place
    public string description; //description of the historical event
    public float severity; //how importance was this event to the npc
    public float timeOfAction; //when this action occurred
    public int performer; //the npc that performed the action
    public int receiver; //the npc that received the action
    public bool wasPositive; //was the action positive for the performer
    public bool wasSuccessful; //was the action positive for the performer
    public float performerImportance; //how important the event is in the performing NPCs mind
    public float receiverImportance; //how important the event is in the receiving NPCs mind

    public BuildingEvent()
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
        performerImportance = perImp;
        receiverImportance = recImp;

    }
}

/*
Wrapper for NPC History Item list that can be used as a stepping stone when serializing and de-serializing
*/
[System.Serializable]
public class BuildingEventDatabase
{
    public List<BuildingEvent> items = new List<BuildingEvent>();
}