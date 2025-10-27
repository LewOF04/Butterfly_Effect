using UnityEngine;
using System.Collections.Generic;

public class DataController : MonoBehaviour
{
    [Header("Managers")]
    public NPCManager npcManager;
    public BuildingManager buildingManager;
    public TraitManager traitManager;

    [Header("House ScrObjs")]
    public BuildingType house1Data;
    public BuildingType house2Data;
    public BuildingType house3Data;
    public BuildingType house4Data;
    public BuildingType house5Data;

    
    public Dictionary<string, NPC> NPCStorage;
    public Dictionary<string, Building> BuildingStorage;
    public Dictionary<int, TraitData> TraitStorage;

    private void Awake()
    {
        //fallbacks in case the references weren’t assigned in the Inspector
        if (!npcManager)      npcManager      = FindObjectOfType<NPCManager>(true);
        if (!buildingManager) buildingManager = FindObjectOfType<BuildingManager>(true);
        if (!traitManager)    traitManager    = FindObjectOfType<TraitManager>(true);
    }

    private void Start()
    {
        //loads traits first as NPCs are reliant on their existence
        TraitStorage = traitManager ? traitManager.LoadTraits() : new Dictionary<int, TraitData>();

        NPCStorage = npcManager ? (npcManager.LoadNPCs() ?? new Dictionary<string, NPC>())
                                        : new Dictionary<string, NPC>();
                                          
        BuildingStorage = buildingManager ? (buildingManager.LoadBuildings() ?? new Dictionary<string, Building>()) 
                                        : new Dictionary<string, Building>();
    }
}