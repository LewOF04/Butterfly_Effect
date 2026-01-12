using UnityEngine;
using System.Collections.Generic;
using System;

public class DataController : MonoBehaviour
{
    public static DataController Instance;

    [Header("Managers")]
    public NPCManager npcManager;
    public BuildingManager buildingManager;
    public TraitManager traitManager;
    public ActionManager actionManager;
    public RelationShipManager relationshipManager;

    [Header("Containers")]
    public Transform npcContainer;
    public Transform buildingContainer;
    public Dictionary<int, NPC> NPCStorage;
    public Dictionary<int, Building> BuildingStorage;
    public Dictionary<int, TraitData> TraitStorage;
    public Dictionary<string, Action> ActionStorage;
    public Dictionary<RelationshipKey, Relationship> RelationshipStorage;
    public Dictionary<id, List<Relationship>> RelationshipByNPCStorage;
    public Dictionary<int, List<int>> NPCBuildingLinks = new Dictionary<int, List<int>>();

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

    public void saveToMemory()
    {
        buildingManager.SaveBuildings(BuildingStorage);
        npcManager.SaveNPCs(NPCStorage);
        RelationshipManager.SaveRelationships(RelationshipStorage);
    }

    public void LoadFromMemory()
    {
        //loads traits first as NPCs are reliant on their existence
        TraitStorage = traitManager.LoadTraits();

        NPCStorage = npcManager.LoadNPCs();

        BuildingStorage = buildingManager.LoadBuildings();

        ActionStorage = actionManager.LoadActions();

        RelationshipStorage, RelationshipByNPCStorage = relationshipManager.LoadRelationships();

        foreach (var (id, building) in BuildingStorage)
        {
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }

    public void linkNPCsAndBuildings(System.Random rng)
    {

        foreach (var(id, npc) in NPCStorage)
        {
            int npcsHouse = rng.Next(0, BuildingStorage.Count-1);
            npc.parentBuilding = npcsHouse;
            BuildingStorage[npcsHouse].inhabitants.Add(npc.id);
        }
    }

    public void seedGenerate(int seed)
    {
        System.Random rng = new System.Random(seed);
        
        TraitStorage = traitManager.LoadTraits(); //loads traits from memory

        ActionStorage = actionManager.LoadActions(); //load actions from memory

        BuildingStorage = buildingManager.generateBuildings(rng); //use rng to produce buildings

        NPCStorage = npcManager.generateNPCs(rng, TraitStorage.Count); //use rng to produce npcs

        RelationshipStorage, RelationshipByNPCStorage = relationshipManager.generateRelationships(rng, NPCStorage); //use rng to create npc relationships

        linkNPCsAndBuildings(rng);
        
        foreach (var (id, building) in BuildingStorage)
        {
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }
}