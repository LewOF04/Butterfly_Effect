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
    public float buildingImportance; //how important this action is for the building
    public float npcImportance; //how important this action is for the npc

    public BuildingEvent(BuildingEventKey key, string name, string desc, float time)
    {
        eventKey = key;
        actionName = name;
        description = desc;
        timeOfAction = time;
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