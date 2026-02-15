using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingEvent
{
    public BuildingEventKey eventKey; //unique key identifier of the key
    public string actionName; //the name of the action that took place
    public string description; //description of the historical event
    public float timeOfAction; //when this action occurred
    public float severity; //how severe this action
    public bool wasPositivePerf; //if the action was positive for the performer
    public bool wasPositiveRec; //if the action was successful for the receiver
    public float buildingImportance; //how important this action is for the building
    public float npcImportance; //how important this action is for the npc 

    public BuildingEvent(BuildingEventKey key, string name, string desc, float time, float sev, bool wasPosPerf, bool wasPosRec, float buildImp, float npcImp)
    {
        eventKey = key;
        actionName = name;
        description = desc;
        timeOfAction = time;
        severity = sev;
        wasPositivePerf = wasPosPerf;
        wasPositiveRec = wasPosRec;
        buildingImportance = buildImp;
        npcImportance = npcImp;
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