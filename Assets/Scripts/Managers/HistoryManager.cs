using UnityEngine;
using System.Collections.Generic;
using System;

public class HistoryManager : MonoBehaviour
{
    private DataController dataController; 

    private void Awake()
    {
        dataController = DataController.Instance;
    }

    public void GenerateHistory(System.Random rng)
    {
        
        Dictionary<int, NPC> npcStorage = dataController.NPCStorage;
        Dictionary<int, Building> buildingStorage = dataController.BuildingStorage;

        //NPC History
        NPCHistoryTracker npcHistoryTracker = dataController.npcHistoryTracker;
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> NPCEventStorage = new Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>>();
        foreach(var kvp in dataController.RelationshipStorage)
        {
            //add to npc history tracker for each relationship with other npcs
            npcHistoryTracker.largestInt[kvp.Key] = 0;
            npcHistoryTracker.missingInts[kvp.Key] = new List<int>();

            //add for event storage location for each npc pair
            NPCEventStorage[kvp.Key] = new Dictionary<NPCEventKey, NPCEvent>();
        }

        Dictionary<int, List<NPCEvent>> eventsPerNPCStorage = new Dictionary<int, List<NPCEvent>>();
        foreach(var kvp in dataController.NPCStorage)
        {
            //add to npc history tracker for npcs self relationships
            RelationshipKey selfRel = new RelationshipKey(kvp.Key, -1); //create a self relationship key
            npcHistoryTracker.largestInt[selfRel] = 0;
            npcHistoryTracker.missingInts[selfRel] = new List<int>();

            //add secondary storage for easy access to each npc information
            eventsPerNPCStorage[kvp.Key] = new List<NPCEvent>();
        }

        dataController.NPCEventStorage = NPCEventStorage;
        dataController.eventsPerNPCStorage = eventsPerNPCStorage;


        //Building History
        BuildingHistoryTracker buildingHistoryTracker = dataController.buildingHistoryTracker;
        Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEventStorage = new Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>>();
        Dictionary<int, List<BuildingEvent>> buildingEventsPerNPCStorage = new Dictionary<int, List<BuildingEvent>>();
        foreach(var npcPair in dataController.NPCStorage)
        {
            buildingEventsPerNPCStorage[npcPair.key] = new List<BuildingEvent>(); //populate per NPC
            foreach(var buildingPair in dataController.BuildingStorage)
            {
                //populate npc and building stores
                BuildingRelationshipKey rel = new BuildingRelationshipKey(buildingPair.Key, npcPair.Key);
                buildingHistoryTracker.largestInt[rel] = 0;
                buildingHistoryTracker.missingInts[rel] = new List<int>();

                buildingEventStorage[rel] = new Dictionary<BuildingEventKey, BuildingEvent>();
            }
        }

        Dictionary<int, List<BuildingEvent>> buildingEventsPerBuildingStorage = new Dictionary<int, List<BuildingEvent>>();
        foreach(var buildingPair in dataController.BuildingStorage)
        {
            //populate overall relationship storage
            BuildingRelationshipKey selfRel = new BuildingRelationshipKey(buildingPair.Key, -1);
            buildingHistoryTracker.largestInt[selfRel] = 0;
            buildingHistoryTracker.missingInts[selfRel] = new List<int>();
            buildingEventStorage[selfRel] = new Dictionary<BuildingEventKey, BuildingEvent>();

            //populate per building storage
            buildingEventsPerBuildingStorage[buildingPair.Key] = new List<BuildingEvent>();
        }

        dataController.buidlingEventStorage = buildingEventStorage;
        dataController.buildingEventsPerNPCStorage = buildingEventsPerNPCStorage;
        dataController.buildingEventsPerBuildingStorage = buildingEventsPerBuildingStorage;
    }

    public void SaveHistory() 
    {
        SaveSys.SaveNPCHistory(dataController.NPCEventStorage);
        SaveSys.SaveNPCHistoryTracker(dataController.npcHistoryTracker);
        SaveSys.SaveBuildingHistory(dataController.buildingEventStorage);
        SaveSys.SaveBuildingHistoryTracker(dataController.buildingHistoryTracker);
    }

    public void LoadHistory()
    {
        //=====================LOADING NPC EVENTS===========================
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = new Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>>();
        Dictionary<int, List<NPCEvent>> perNPCEvents = new Dictionary<int, List<NPCEvent>>();

        List<NPCEvent> npcEventsList = SaveSys.LoadNPCHistory();

        //take each stored NPCEvent and move it into the necessary storage structures
        foreach(NPCEvent anEvent in npcEventsList)
        {
            NPCEventKey thisKey = anEvent.eventKey; //get the unique event key
            RelationshipKey relKey = new RelationshipKey(thisKey.npcA, thisKey.npcB); //get the relationship for these two npcs

            //create the secondary dictionary if it is not already present
            if (!npcEvents.ContainsKey(relKey))
            {
                npcEvents[relKey] = new Dictionary<NPCEventKey, NPCEvent>();
            }
            npcEvents[relKey][thisKey] = anEvent; //store the event

            //create the lists in perNPCEvents if not present
            if (!perNPCEvents.ContainsKey(thisKey.npcA))
            {
                perNPCEvents[thisKey.npcA] = new List<NPCEvent>();
            }
            if (!perNPCEvents.ContainsKey(thisKey.npcB))
            {
                perNPCEvents[thisKey.npcB] = new List<NPCEvent>();
            }

            //add event to both per NPC events
            perNPCEvents[thisKey.npcA].Add(anEvent);
            perNPCEvents[thisKey.npcB].Add(anEvent);
        }

        //update in the data controller
        dataController.NPCEventStorage = npcEvents;
        dataController.eventsPerNPCStorage = perNPCEvents;


        //=====================LOADING NPC HISTORY TRACKER===========================
        NPCHistoryTrackerDatabase db = SaveSys.LoadNPCHistoryTracker();
        dataController.npcHistoryTracker.SetValues(db.largestInts, db.largestIntKeys, db.missingInts, db.missingIntsKeys);
        NPCHistoryTracker npcHistoryTracker = dataController.npcHistoryTracker;

        //trim down missing lists to as small as possible
        var keys = new List<RelationshipKey>(npcHistoryTracker.largestInt.Keys);
        foreach(var rel in keys) //iterate overl keys
        {
            int largInt = npcHistoryTracker.largestInt[rel];
            List<int> list = npcHistoryTracker.missingInts[rel];

            while (list.Remove(largInt - 1)) 
            {
                //if one less than the largest int is in it, remove that and set it to be the largest int
                largInt--;
            }
            npcHistoryTracker.largestInt[rel] = largInt;
        }

        dataController.npcHistoryTracker = npcHistoryTracker;
    }

    /*
    Removes the memory item from the NPCs memory when it is no longer important enough to remember.
    Removes memory from storage overall if there is no memory at all
    */
    private void RemoveMemoryFromNPC(NPCEvent theEvent, int npcID)
    {
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;

        List<NPCEvent> thisNPCEvents = perNPCEvents[npcID];
        int index = 0;
        foreach(NPCEvent anEvent in thisNPCEvents)
        {
            if(anEvent == theEvent)
            {
                thisNPCEvents.RemoveAt(index); //remove memory from this npc's memory

                //remove memory entirely if there is no other npc remembering it
                if(theEvent.eventKey.npcB == -1)
                {
                    RelationshipKey relKey = new RelationshipKey(npcID, -1);

                    Dictionary<NPCEventKey, NPCEvent> subDict = npcEvents[relKey];
                    dataController.npcHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                    subDict.Remove(theEvent.eventKey);
                }
                else
                {
                    int secondNPC;
                    if(theEvent.eventKey.npcA == npcID) secondNPC = theEvent.eventKey.npcB;
                    else secondNPC = theEvent.eventKey.npcA;

                    bool shouldRemove = !perNPCEvents[secondNPC].Contains(theEvent); //check whether it is present in the other npc
                    
                    if(shouldRemove){
                        RelationshipKey relKey = new RelationshipKey(secondNPC, npcID);
                        
                        Dictionary<NPCEventKey, NPCEvent> subDict = npcEvents[relKey];
                        dataController.npcHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                        subDict.Remove(theEvent.eventKey);
                    }
                }
                break;
            }
            index++;
        }
    }

    /*
    Adds an NPC memory to the stores

    @param actionName - the name of the action that occurred
    @param description - the description of the action occurrence
    @param severity - the severity of the action
    @param timeSinceAction - how long ago this action occurred (will mostly be 0 as these are new actions)
    @param performer - the NPC who performed the action
    @param receiver - the NPC who was the receiver of the action (-1 if the action is performed on themself)
    @param wasPositive - was the action positive or negative for the performer?
    @param wasSuccessful - was the action successful for the performer?
    */
    public void AddNPCMemory(string actionName, string description, float severity, float timeOfAction, int performer, int receiver, bool wasPositive, bool wasSuccessful)
    {

        RelationshipKey relKey = new RelationshipKey(performer, receiver);

        NPCHistoryTracker tracker = dataController.npcHistoryTracker;

        //determine the key of this event
        int nextInt = tracker.getNextInt(relKey);
        NPCEventKey key = new NPCEventKey(performer, receiver, nextInt);

        float performerImportance = calculateImportance(severity, timeOfAction, performer); //determine the importance of this event to the performer
        float receiverImportance = calculateImportance(severity, timeOfAction, receiver); //determine the importance of this event to the receiver

        NPCEvent thisEvent = new NPCEvent(key, actionName, description, severity, timeOfAction, performer, receiver, wasPositive, wasSuccessful, performerImportance, receiverImportance);

        //add to per NPC storage
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;
        perNPCEvents[performer].Add(thisEvent);
        perNPCEvents[receiver].Add(thisEvent);

        //add to overall storage
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        npcEvents[relKey][key] = thisEvent;
    }

    /*
    Calculate the importance of an action based on how severe it was and how long ago it happened.

    A lesser importance will result in it being forgotten quicker.
    */
    public float calculateImportance(float sev, float time, int npcID)
    {
        //if there is no NPC then return -1 for a void 
        if(npcID == -1)
        {
            return -1;
        }
        //TODO: need to implement (need to reference current time vs then time and NPC memory size)
        return sev;
    }

    /*
    Delete unimportant events from memory of NPCs
    */
    public void deleteUnimportantEvents()
    {
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        List<RelationshipKey> relKeys = new List<RelationshipKey>(npcEvents.Keys);

        foreach(RelationshipKey rel in relKeys)
        {
            Dictionary<NPCEventKey, NPCEvent> innerDic = npcEvents[rel];
            List<NPCEventKey> eventKeys = new List<NPCEventKey>(innerDic.Keys);
            foreach(NPCEventKey eventKey in eventKeys)
            {
                NPCEvent currEvent = innerDic[eventKey];

                currEvent.performerImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.performer);
                currEvent.receiverImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.receiver);

                if(currEvent.performerImportance < 2)
                {
                    RemoveMemoryFromNPC(currEvent, currEvent.performer);
                } 
                if(currEvent.receiverImportance < 2)
                {
                    RemoveMemoryFromNPC(currEvent, currEvent.receiver);
                }
            }
        }
    }
}