using System.Collections.Generic;

public interface IDataContainer
{
    Dictionary<int, IAgent> NPCStorage { get; set; }
    Dictionary<int, IBuilding> BuildingStorage { get; set; }
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
    IWorldData worldManager { get; set; }
}