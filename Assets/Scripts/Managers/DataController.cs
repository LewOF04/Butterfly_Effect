using UnityEngine;
using System.Collections.Generic;
using System;

public class DataController : MonoBehaviour
{
    public static DataController Instance;

    [Header("Managers")]
    //managers for each data type that contain related functions
    public NPCManager npcManager;
    public BuildingManager buildingManager;
    public TraitManager traitManager;
    public ActionManager actionManager;
    public RelationshipManager relationshipManager;
    public HistoryManager historyManager;
    public NPCHistoryTracker npcHistoryTracker; //npcHistoryTracker to record npc related interactions
    public BuildingHistoryTracker buildingHistoryTracker; //buildingHistoryTracker to record building related interactions

    [Header("Runtime Containers")]
    //containers to hold instantiated buildings and npcs
    public Transform npcContainer;
    public Transform buildingContainer;

    [Header("Runtime Storage")]
    //storage containers for easy access to npcs, buildings, traits and actions
    public Dictionary<int, NPC> NPCStorage;
    public Dictionary<int, Building> BuildingStorage;
    public Dictionary<int, TraitData> TraitStorage;
    public Dictionary<string, IActionBase> ActionStorage;

    [Header("<i>Relationship</i>")]
    public Dictionary<RelationshipKey, Relationship> RelationshipStorage; //dictionary that stores every relationship
    public Dictionary<int, List<Relationship>> RelationshipPerNPCStorage; //dicionary that stores the relationships indexable by each npc

    [Header("<i>Memory</i>")]
    public Dictionary<int, List<NPCEvent>> eventsPerNPCStorage; //which events each NPC remember
    public Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> NPCEventStorage; 
    public Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> buildingEventStorage;
    public Dictionary<int, List<BuildingEvent>> buildingEventsPerBuildingStorage; 
    public Dictionary<int, List<BuildingEvent>> buildingEventsPerNPCStorage;

    [Header("Actions")]
    public List<BuildingAction> buildingActions;
    public List<NPCAction> npcActions;
    public List<SelfAction> selfActions;
    public List<EnvironmentAction> environmentActions;

    [Header("Other stores")]
    public Dictionary<int, List<int>> NPCBuildingLinks = new Dictionary<int, List<int>>();

    [Header("Game Variables")]
    public WorldManager worldManager;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(Instance);}
        Instance = this;
        this.transform.SetParent(null); //removes its parent, making it a root prefab
        DontDestroyOnLoad(gameObject); //this won't be destroyed on reload

        if (npcContainer == null)
        {
            var newContainer = new GameObject("NPCContainer");
            newContainer.transform.SetParent(transform, false);
            npcContainer = newContainer.transform;
        }
        if (buildingContainer == null)
        {
            var newContainer = new GameObject("BuildingContainer");
            newContainer.transform.SetParent(transform, false);
            buildingContainer = newContainer.transform;
        }
    }

    public void SaveToMemory()
    {
        buildingManager.SaveBuildings();
        npcManager.SaveNPCs();
        relationshipManager.SaveRelationships();
        historyManager.SaveHistory();
        worldManager.SaveWorldData();
    }

    /*
    Load the save file for each relevant data set
    */
    public void LoadFromMemory()
    {
        //loads traits first as NPCs are reliant on their existence
        traitManager.LoadTraits();

        npcManager.LoadNPCs();

        buildingManager.LoadBuildings();

        actionManager.LoadActions();

        relationshipManager.LoadRelationships();

        historyManager.LoadHistory();

        worldManager.LoadWorldData();
        
        foreach (var (id, building) in BuildingStorage)
        {
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }

    /*
    Function to randomly link NPCs with houses once they have all been instantiated following generation
    */
    private void linkNPCsAndBuildings(System.Random rng)
    {

        foreach (var(id, npc) in NPCStorage)
        {
            int npcsHouse = rng.Next(0, BuildingStorage.Count-1);
            npc.parentBuilding = npcsHouse;
            BuildingStorage[npcsHouse].inhabitants.Add(npc.id);
        }
    }

    /*
    Generate world data including Buildings, NPCs and relationships based upon the provided seed.
    */
    public void seedGenerate(int seed)
    {
        Debug.Log("Generated Seed: "+seed.ToString());
        System.Random rng = new System.Random(seed);
        
        traitManager.LoadTraits(); //loads traits from memory

        actionManager.LoadActions(); //load actions from memory

        buildingManager.GenerateBuildings(rng); //use rng to produce buildings

        npcManager.GenerateNPCs(rng, TraitStorage.Count); //use rng to produce npcs

        relationshipManager.GenerateRelationships(rng); //use rng to create npc relationships

        historyManager.GenerateHistory(rng); //generate history and NPC history tracker

        worldManager.GenerateWorldData(rng); //generate world data

        linkNPCsAndBuildings(rng);
        
        foreach (var (id, building) in BuildingStorage)
        {
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }
}