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
    public WorldData worldManager;

    public DataControllerSnapshot()
    {
        DataController dataController = DataController.Instance;

        //base storage cloning
        NPCStorage = dataController.npcManager.DeepClone();
        BuildingStorage = dataController.buildingManager.DeepClone();
        TraitStorage = dataController.TraitStorage;

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

    IWorldData IDataContainer.World { get => worldManager; }

    //--------------------Interface Methods--------------------
    //====================Agents====================
    public IEnumerable<IAgent> Agents => NPCStorage.Values; 
    public int AgentCount => NPCStorage.Count;

    public bool ContainsAgent(int id) => NPCStorage.ContainsKey(id);

    public bool TryGetAgent(int id, out IAgent agent)
    {
        if (NPCStorage.TryGetValue(id, out NPCData npc))
        {
            agent = npc;
            return true;
        }

        agent = null;
        return false;
    }

    public bool TryAddAgent(IAgent agent)
    {
        if (agent is not NPCData npc) return false;
        if (NPCStorage.ContainsKey(npc.id)) return false;

        NPCStorage.Add(npc.id, npc);
        return true;
    }

    public bool TryRemoveAgent(int id) => NPCStorage.Remove(id);

    //====================Buildings====================
    public IEnumerable<IBuilding> Buildings => BuildingStorage.Values;
    public int BuildingCount => BuildingStorage.Count;

    public bool ContainsBuilding(int id) => BuildingStorage.ContainsKey(id);

    public bool TryGetBuilding(int id, out IBuilding building)
    {
        if (BuildingStorage.TryGetValue(id, out BuildingData b))
        {
            building = b;
            return true;
        }

        building = null;
        return false;
    }

    public bool TryAddBuilding(IBuilding building)
    {
        if (building is not BuildingData b) return false;
        if (BuildingStorage.ContainsKey(b.id)) return false;

        BuildingStorage.Add(b.id, b);
        return true;
    }

    public bool TryRemoveBuilding(int id) => BuildingStorage.Remove(id);
}