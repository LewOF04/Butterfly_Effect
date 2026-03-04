using System.Collections.Generic;

public interface IDataContainer
{
    //Dictionary<int, IAgent> NPCStorage { get; set; }
    //Dictionary<int, IBuilding> BuildingStorage { get; set; }
    Dictionary<int, TraitData> TraitStorage { get; set; }
    Dictionary<string, IAction> ActionStorage { get; set; }

    Dictionary<RelationshipKey, Relationship> RelationshipStorage { get; set; }
    Dictionary<int, List<Relationship>> RelationshipPerNPCStorage { get; set; }

    Dictionary<int, List<NPCEvent>> eventsPerNPCStorage { get; set; }
    Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> NPCEventStorage { get; set; }
    Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEventStorage { get; set; }
    Dictionary<int, List<BuildingEvent>> buildingEventsPerBuildingStorage { get; set; }
    Dictionary<int, List<BuildingEvent>> buildingEventsPerNPCStorage { get; set; }

    Dictionary<int, List<int>> NPCBuildingLinks { get; set; }
    IWorldData World { get; }

    NPCHistoryTracker npcHistoryTracker { get; set; }
    BuildingHistoryTracker buildingHistoryTracker { get; set; }

    //IAgent and IBuilding differences 
    //Agents
    IEnumerable<IAgent> Agents { get; }
    int AgentCount { get; }
    bool ContainsAgent(int id);
    bool TryGetAgent(int id, out IAgent agent);

    bool TryAddAgent(IAgent agent);
    bool TryRemoveAgent(int id);

    //Buildings
    IEnumerable<IBuilding> Buildings { get; }
    int BuildingCount { get; }
    bool ContainsBuilding(int id);
    bool TryGetBuilding(int id, out IBuilding building);

    bool TryAddBuilding(IBuilding building);
    bool TryRemoveBuilding(int id);
}