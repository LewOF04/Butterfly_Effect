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

    [Header("Containers")]
    public Transform npcContainer;
    public Transform buildingContainer;
    public Dictionary<int, NPC> NPCStorage;
    public Dictionary<int, Building> BuildingStorage;
    public Dictionary<int, TraitData> TraitStorage;
    public Dictionary<int, List<int>> NPCBuildingLinks = new Dictionary<int, List<int>>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(Instance);}
        Instance = this;
        this.transform.SetParent(null); //removes its parent, making it a root prefab
        DontDestroyOnLoad(gameObject);

        if (npcContainer == null)
        {
            var go = new GameObject("NPCContainer");
            go.transform.SetParent(transform, false);
            npcContainer = go.transform;
        }
        if (buildingContainer == null)
        {
            var go = new GameObject("BuildingContainer");
            go.transform.SetParent(transform, false);
            buildingContainer = go.transform;
        }
    }

    public void saveToMemory()
    {
        buildingManager.SaveBuildings(BuildingStorage);
        npcManager.SaveNPCs(NPCStorage);
    }

    public void LoadFromMemory()
    {
        //loads traits first as NPCs are reliant on their existence
        TraitStorage = traitManager ? traitManager.LoadTraits() : new Dictionary<int, TraitData>();

        NPCStorage = npcManager ? (npcManager.LoadNPCs() ?? new Dictionary<int, NPC>())
                                        : new Dictionary<int, NPC>();

        BuildingStorage = buildingManager ? (buildingManager.LoadBuildings() ?? new Dictionary<int, Building>())
                                        : new Dictionary<int, Building>();

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
        Debug.Log("Entered seed generate");
        System.Random rng = new System.Random(seed);
        Debug.Log("Random created");
        TraitStorage = traitManager ? traitManager.LoadTraits() : new Dictionary<int, TraitData>(); //loads traits from memory
        Debug.Log("Traits Created");

        BuildingStorage = buildingManager.generateBuildings(rng);
        Debug.Log("Buildings Created");

        NPCStorage = npcManager.generateNPCs(rng, TraitStorage.Count);
        Debug.Log("NPCs Created");

        linkNPCsAndBuildings(rng);
        
        foreach (var (id, building) in BuildingStorage)
        {
            Debug.Log("Iteration");
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }
}