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
        GenerateHistoryFramework();
    }

    public void GenerateHistoryFramework()
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
            buildingEventsPerNPCStorage[npcPair.Key] = new List<BuildingEvent>(); //populate per NPC
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

        dataController.buildingEventStorage = buildingEventStorage;
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
        GenerateHistoryFramework(); //ensure all data structures are initialised

        //=====================LOADING NPC EVENTS===========================
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;

        List<NPCEvent> npcEventsList = SaveSys.LoadNPCHistory();

        //take each stored NPCEvent and move it into the necessary storage structures
        foreach(NPCEvent anEvent in npcEventsList)
        {
            NPCEventKey thisKey = anEvent.eventKey; //get the unique event key
            RelationshipKey relKey = new RelationshipKey(thisKey.npcA, thisKey.npcB); //get the relationship for these two npcs

            npcEvents[relKey][thisKey] = anEvent; //store the event

            //add event to both per NPC events
            perNPCEvents[thisKey.npcA].Add(anEvent);
            perNPCEvents[thisKey.npcB].Add(anEvent);
        }

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


        //=============LOADING BUILDING HISTORY===============================
        Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEventStorage = dataController.buildingEventStorage;
        Dictionary<int, List<BuildingEvent>> buildingEventsPerBuildingStorage = dataController.buildingEventsPerBuildingStorage; 
        Dictionary<int, List<BuildingEvent>> buildingEventsPerNPCStorage = dataController.buildingEventsPerNPCStorage;

        List<BuildingEvent> buildingEventList = SaveSys.LoadBuildingEvents();

        foreach(var buildingEvent in buildingEventList)
        {
            BuildingEventKey key = buildingEvent.eventKey;
            BuildingRelationshipKey relKey = new BuildingRelationshipKey(key.building, key.npc);

            buildingEventStorage[relKey][key] = buildingEvent;
            buildingEventsPerBuildingStorage[key.building].Add(buildingEvent);

            if(key.npc != -1)
            {
                buildingEventsPerNPCStorage[key.npc].Add(buildingEvent);
            }
        }


        //============LOADING BUILDING HISTORY TRACKER=======================
        BuildingHistoryTrackerDatabase buildDB = SaveSys.LoadBuildingHistoryTracker();
        dataController.buildingHistoryTracker.SetValues(buildDB.largestInts, buildDB.largestIntKeys, buildDB.missingInts, buildDB.missingIntsKeys);
        BuildingHistoryTracker buildingHistoryTracker = dataController.buildingHistoryTracker;

        //trim down missing lists to as small as possible
        var buildingKeys = new List<BuildingRelationshipKey>(buildingHistoryTracker.largestInt.Keys);
        foreach(var rel in buildingKeys) //iterate overl keys
        {
            int largInt = buildingHistoryTracker.largestInt[rel];
            List<int> list = buildingHistoryTracker.missingInts[rel];

            while (list.Remove(largInt - 1)) 
            {
                //if one less than the largest int is in it, remove that and set it to be the largest int
                largInt--;
            }
            buildingHistoryTracker.largestInt[rel] = largInt;
        }

        dataController.buildingHistoryTracker = buildingHistoryTracker;
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
    Removes the memory item from the building/npc memory when it is no longer important enough to remember.
    Removes memory from storage overall if there is no memory at all
    */
    private void RemoveMemoryFromBuilding(BuildingEvent theEvent, bool isBuilding)
    {
        int id;
        if(isBuilding) id = theEvent.eventKey.building;
        else id = theEvent.eventKey.npc;
        if(id == -1) return;

        Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEvents = dataController.buildingEventStorage;
        Dictionary<int, List<BuildingEvent>> perEvents;
        Dictionary<int, List<BuildingEvent>> perOther;

        if(isBuilding) {
            perEvents = dataController.buildingEventsPerBuildingStorage;
            perOther = dataController.buildingEventsPerNPCStorage;
        }
        else
        {
            perEvents = dataController.buildingEventsPerNPCStorage;
            perOther = dataController.buildingEventsPerBuildingStorage;
        } 

        List<BuildingEvent> thisEvents = perEvents[id];
        int index = 0;
        foreach(BuildingEvent anEvent in thisEvents)
        {
            if(anEvent == theEvent)
            {
                thisEvents.RemoveAt(index); //remove memory from the memory

                if (isBuilding)
                {   
                    //remove memory entirely if there is no npc remembering it
                    if(theEvent.eventKey.npc == -1)
                    {
                        BuildingRelationshipKey relKey = new BuildingRelationshipKey(id, -1);

                        Dictionary<BuildingEventKey, BuildingEvent> subDict = buildingEvents[relKey];
                        dataController.buildingHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                        subDict.Remove(theEvent.eventKey);
                    }
                    else
                    {
                        int npcID = theEvent.eventKey.npc;

                        bool shouldRemove = !perOther[npcID].Contains(theEvent); //check whether it is present in the other npc
                        
                        if(shouldRemove){
                            BuildingRelationshipKey relKey = new BuildingRelationshipKey(id, npcID);
                            
                            Dictionary<BuildingEventKey, BuildingEvent> subDict = buildingEvents[relKey];
                            dataController.buildingHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                            subDict.Remove(theEvent.eventKey);
                        }
                    }
                }
                else
                {
                    int npcID = theEvent.eventKey.npc;
                    int buildingID = theEvent.eventKey.building;

                    bool shouldRemove = !perOther[npcID].Contains(theEvent); //check whether it is present in the other npc
                        
                    if(shouldRemove){
                        BuildingRelationshipKey relKey = new BuildingRelationshipKey(buildingID, id);
                        
                        Dictionary<BuildingEventKey, BuildingEvent> subDict = buildingEvents[relKey];
                        dataController.buildingHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                        subDict.Remove(theEvent.eventKey);
                    }
                }
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

        bool storeForPerformer = keepNPCMemory(performerImportance, performer);
        bool storeForReceiver = keepNPCMemory(receiverImportance, receiver);

        if(!(storeForPerformer || storeForReceiver)) return; //if this won't be stored for either

        NPCEvent thisEvent = new NPCEvent(key, actionName, description, severity, timeOfAction, performer, receiver, wasPositive, wasSuccessful, performerImportance, receiverImportance);

        //add to per NPC storage
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;
        if(storeForPerformer) perNPCEvents[performer].Add(thisEvent);
        if(storeForReceiver) perNPCEvents[receiver].Add(thisEvent);

        //add to overall storage
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        npcEvents[relKey][key] = thisEvent;
    }

     
    /*
    Adds a Building memory to the stores

    @param name - the name of the action
    @param desc - the description of the action
    @param severity - the severity of the action
    @param time - the time when the action occurred
    @param building - the id of the building
    @param npc - the id of the npc
    @param wasSucc - whether or not the action was successful as intended
    @param wasPos - whether or not this action was positive for the building
    */
    public void AddBuildingMemory(string name, string desc, float severity, float time, int building, int npc, bool wasSucc, bool wasPos)
    {
        BuildingRelationshipKey relKey = new BuildingRelationshipKey(building, npc);

        BuildingHistoryTracker tracker = dataController.buildingHistoryTracker;

        //determine the key of this event
        int nextInt = tracker.getNextInt(relKey);
        BuildingEventKey key = new BuildingEventKey(npc, building, nextInt);

        float buildingImportance = calculateImportance(severity, time, building);
        float npcImportance = calculateImportance(severity, time, npc);

        bool storeForBuilding = keepBuildingMemory(buildingImportance, building, true);
        bool storeForNPC = keepBuildingMemory(npcImportance, npc, false);

        if(!(storeForBuilding || storeForNPC)) return;

        BuildingEvent thisEvent = new BuildingEvent(key, name, desc, time, severity, wasSucc, wasPos, buildingImportance, npcImportance);

        //add to per Building Storage
        Dictionary<int, List<BuildingEvent>> perBuilding = dataController.buildingEventsPerBuildingStorage;
        if(storeForBuilding) perBuilding[building].Add(thisEvent);

        //add to per npc storage
        Dictionary<int, List<BuildingEvent>> perNPC = dataController.buildingEventsPerNPCStorage;
        if(storeForNPC) perNPC[npc].Add(thisEvent);

        //add to building events storage
        Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEvents = dataController.buildingEventStorage;
        buildingEvents[relKey][key] = thisEvent;
    }

    /*
    Calculate the importance of an action based on how severe it was and how long ago it happened
    */
    public float calculateImportance(float sev, float time, int ID)
    {
        if(ID == -1)
        {
            return -1;
        }

        float worldTime = dataController.worldManager.gameTime;
        float timeDiff = worldTime - time;

        return(10/timeDiff) * sev;
    }

    /*
    Delete unimportant events from memory of NPCs and Buildings
    */
    public void deleteUnimportantEvents()
    {
        //=========Delete unimportant NPC events===========
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        List<RelationshipKey> relKeys = new List<RelationshipKey>(npcEvents.Keys);

        foreach(RelationshipKey rel in relKeys)
        {
            Dictionary<NPCEventKey, NPCEvent> innerDict = npcEvents[rel];
            List<NPCEventKey> eventKeys = new List<NPCEventKey>(innerDict.Keys);
            foreach(NPCEventKey eventKey in eventKeys)
            {
                NPCEvent currEvent = innerDict[eventKey];

                currEvent.performerImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.performer);
                currEvent.receiverImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.receiver);

                if(!keepNPCMemory(currEvent.performerImportance, currEvent.performer)) RemoveMemoryFromNPC(currEvent, currEvent.performer);
                
                if(!keepNPCMemory(currEvent.receiverImportance, currEvent.receiver)) RemoveMemoryFromNPC(currEvent, currEvent.receiver);
            }
        }

        //=======Delete unimportant Building Events=========
        Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEvents = dataController.buildingEventStorage;
        List<BuildingRelationshipKey> buildRelKeys = new List<BuildingRelationshipKey>(buildingEvents.Keys);

        foreach(BuildingRelationshipKey rel in buildRelKeys)
        {
            Dictionary<BuildingEventKey, BuildingEvent> innerDict = buildingEvents[rel];
            List<BuildingEventKey> eventKeys = new List<BuildingEventKey>(innerDict.Keys);
            foreach(BuildingEventKey eventKey in eventKeys)
            {
                BuildingEvent currEvent = innerDict[eventKey];

                currEvent.buildingImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.eventKey.building);
                currEvent.npcImportance = calculateImportance(currEvent.severity, currEvent.timeOfAction, currEvent.eventKey.npc);

                if(!keepBuildingMemory(currEvent.buildingImportance, currEvent.eventKey.building, true)) RemoveMemoryFromBuilding(currEvent, true);
                if(!keepBuildingMemory(currEvent.npcImportance, currEvent.eventKey.npc, true)) RemoveMemoryFromBuilding(currEvent, false);
            }
        }
    }

    /*
    Given a building or npc and the relative importance of that action to them. Determine whether or not this will be stored

    @param relativeImportance - the relative importance of the event given time and severity
    @param ID - the id of the building or NPC
    @param isBuilding - whether this is a building or npc

    @return - whether this should be memorised or not
    */
    public bool keepBuildingMemory(float relativeImportance, int ID, bool isBuilding)
    {
        if (ID == -1) return false;

        float total = 0;
        int num = 0;
        List<BuildingEvent> memory;
        if(isBuilding) memory = dataController.buildingEventsPerBuildingStorage[ID];
        else memory = dataController.buildingEventsPerNPCStorage[ID];

        foreach(BuildingEvent anEvent in memory)
        {
            if(isBuilding) total += anEvent.buildingImportance;
            else total += anEvent.npcImportance;
            num++;
        }
        
        if(relativeImportance > (total/num)) return true;

        if(num < 10 && !isBuilding) return true; //NPC will store a minimum of 10 memories about buildings
        if(num < 20 && isBuilding) return true; //building will store a minimum of 20 memories 

        return false;
    }

    /*
    Given an npc and the relative importance of that action to them. Determine whether or not the NPC will remember this or not

    @param relativeImportance - the relative importance of the event given time and severity
    @param npcID - the npc that we're concerned about

    @return - whether or not this memory should be kept
    */
    public bool keepNPCMemory(float relativeImportance, int npcID)
    {
        if(npcID == -1) return false;

        float total = 0;
        int num = 0;
        List<NPCEvent> npcMemory = dataController.eventsPerNPCStorage[npcID];

        foreach(NPCEvent anEvent in npcMemory)
        {
            bool isPerformer = anEvent.performer == npcID;
            if(isPerformer) total += anEvent.performerImportance;
            else total += anEvent.receiverImportance;
            num++;
        }
        
        if(relativeImportance > (total/num)) return true;
        if(num < 20) return true;

        return false;
    }
}