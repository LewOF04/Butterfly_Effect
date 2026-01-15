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

        NPCHistoryTracker npcHistoryTracker = new NPCHistoryTracker();
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

        

        //implement selection

        dataController.npcHistoryTracker = npcHistoryTracker;
        dataController.NPCEventStorage = NPCEventStorage;
        dataController.eventsPerNPCStorage = eventsPerNPCStorage;

    }

    public void SaveHistory() 
    {
        SaveSys.SaveNPCHistory(dataController.NPCEventStorage);
        SaveSys.SaveNPCHistoryTracker(dataController.npcHistoryTracker);
    }

    public void LoadHistory()
    {
        //=====================LOADING NPC EVENTS===========================
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents;
        Dictionary<int, List<NPCEvent>> perNPCEvents;

        List<NPCEvent> npcEvents = SaveSys.LoadNPCHistory();

        //take each stored NPCEvent and move it into the necessary storage structures
        foreach(NPCEvent anEvent in npcEvents)
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
        dataController.NPCEventStorage = NPCEventStorage;
        dataController.eventsPerNPCStorage = eventPerNPC;


        //=====================LOADING NPC HISTORY TRACKER===========================
        NPCHistoryTrackerDatabase db = SaveSys.LoadNPCHistoryTracker();
        NPCHistoryTracker npcHistoryTracker = new NPCHistoryTracker(db.largestInt, db.largestIntKeys, db.missingInts, db.missingIntsKeys);

        /*
        foreach(var kvp in npcHistoryTracker.missingInts)
        {
            kvp.Value.TrimExcess();
        }*/

        dataController.npcHistoryTracker = npcHistoryTracker;
    }

    /*
    Removes the memory item from the NPCs memory when it is no longer important enough to remember.
    Removes memory from storage overall if there is no memory at all
    */
    private void RemoveMemoryFromNPC(NPCEvent theEvent, NPC npc)
    {
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;

        List<NPCEvent> thisNPCEvents = perNPCEvents[npc.id];
        int index = 0;
        foreach(NPCEvent anEvent in thisNPCEvents)
        {
            if(anEvent == theEvent)
            {
                thisNPCEvents.RemoveAt(index); //remove memory from this npc's memory

                //remove memory entirely if there is no other npc remembering it
                if(theEvent.npcB == -1)
                {
                    RelationshipKey relKey = new RelationshipKey(npc.id, -1);

                    Dictionary<NPCEventKey, NPCEvent> subDict = npcEvents[relKey];
                    dataController.npcHistoryTracker.AddRemovedInt(relKey, theEvent.eventKey.eventNum); //remove unused integer
                    subDict.Remove(theEvent.eventKey);
                }
                else
                {
                    int secondNPC;
                    theEvent.npcA == npc.id ? secondNPC = theEvent.npcB : secondNPC = theEvent.npcA;

                    bool shouldRemove = !perNPCEvents[secondNPC].Contains(theEvent); //check whether it is present in the other npc
                    
                    if(shouldRemove){
                        RelationshipKey relKey = new RelationshipKey(secondNPC, npc.id);
                        
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
    public void AddNPCMemory(string actionName, string description, float severity, float timeSinceAction, int performer, int receiver, bool wasPositive, bool wasSuccessful)
    {
        Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> npcEvents = dataController.NPCEventStorage;
        Dictionary<int, List<NPCEvent>> perNPCEvents = dataController.eventsPerNPCStorage;

        RelationshipKey relKey = new RelationshipKey(theEvent.performer.id, theEvent.receiver.id);
        
        

    }
}