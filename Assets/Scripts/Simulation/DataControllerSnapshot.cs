using System;
using System.Collections.Generic;

[Serializable]
public class DataControllerSnapshot : IDataContainer
{
    public Dictionary<int, NPCData> NPCStorage { get; set; }
    public Dictionary<int, BuildingData> BuildingStorage { get; set; }
    public Dictionary<int, TraitData> TraitStorage { get; set; }
    public Dictionary<string, IAction> ActionStorage { get; set; }

    public Dictionary<RelationshipKey, Relationship> RelationshipStorage { get; set; }
    public Dictionary<int, List<Relationship>> RelationshipPerNPCStorage { get; set; }

    public Dictionary<int, List<NPCEvent>> eventsPerNPCStorage { get; set; }
    public Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> NPCEventStorage { get; set; }
    public Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEventStorage { get; set; }
    public Dictionary<int, List<BuildingEvent>> buildingEventsPerBuildingStorage { get; set; }
    public Dictionary<int, List<BuildingEvent>> buildingEventsPerNPCStorage { get; set; }

    public Dictionary<int, List<int>> NPCBuildingLinks { get; set; }
    public WorldData worldManager { get; set; }

    public DataControllerSnapshot()
    {
        DataController dataController = DataController.Instance;

        //base storage cloning
        NPCStorage = dataController.npcManager.DeepClone();
        BuildingStorage = dataController.buildingManager.DeepClone();
        TraitStorage = dataController.traitStorage;

        //relationship cloning
        RelationshipCopyWrapper relCopies = dataController.relationshipManager.DeepClone();
        RelationshipStorage = relCopies.relStorage;
        RelationshipPerNPCStorage = relCopies.relByNPCStorage;

        //history cloning
        HistoryCopyWrapper historyCopies = dataController.historyManager.DeepClone();
        eventsPerNPCStorage = historyCopies.eventsPerNPC;
        NPCEventStorage = historyCopies.NPCEventStorage;
        buildingEventStorage = historyCopies.buildingEventStorage;
        buildingEventsPerBuildingStorage = historyCopies.buildingEventsPerBuildingStorage;
        buildingEventsPerNPCStorage = historyCopies.buildingEventsPerNPCStorage;

        //NPC Building Links cloning
        NPCBuildingLinks = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> oldNPCBuildingLinks = dataController.NPCBuildingLinks;
        foreach(var kvp in oldNPCBuildingLinks) NPCBuildingLinks[kvp.Key] = new List<int>(kvp.Value);

        worldManager = dataController.worldManager.DeepClone();
    }
}