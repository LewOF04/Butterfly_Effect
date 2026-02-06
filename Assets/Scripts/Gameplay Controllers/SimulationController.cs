using UnityEngine;
using System.Collections.Generic;
using System;

public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance;
    private WorldManager worldManager;
    private DataController dataController;
    public BuildingType[] buildingTypes;

    public void Awake()
    {
        dataController = DataController.Instance;
        worldManager = dataController.worldManager;
    }

    public void initiateSimulation(int simulationDays)
    {
        Dictionary<int, NPC> npcDict = dataController.NPCStorage;
        Dictionary<int, Building> buildings = dataController.BuildingStorage;
        Dictionary<RelationshipKey, Relationship> relationships = dataController.RelationshipStorage;
        Dictionary<int, List<NPCEvent>> npcEvents = dataController.eventsPerNPCStorage;
        Dictionary<int, List<BuildingEvent>> perBuildingBuildingEvents = dataController.buildingEventsPerBuildingStorage;
        Dictionary<int, List<BuildingEvent>> perNPCBuildngEvents = dataController.buildingEventsPerNPCStorage;

        float dayTime;
        for(int i = 0; i < simulationDays; i++)
        {
            dayTime = 24.0f;
            List<NPC> npcs = new List<NPC>(npcDict.Values);
            foreach(NPC npc in npcs)
            {
                collectActions(npc, dayTime);
            }
        }
    }

    public ActionFrontier collectActions(NPC performer, float timeLeft)
    {
        ActionFrontier collection = new ActionFrontier(dataController);
        IActionBase bestAction;
        Dictionary<int, List<ActionInfoWrapper>> buildingActions = collection.buildingActions;
        Dictionary<int, List<ActionInfoWrapper>> npcActions = collection.npcActions;
        List<ActionInfoWrapper> environmentActions = collection.environmentActions;
        List<ActionInfoWrapper> selfActions = collection.selfActions;

        return null;
    }
}