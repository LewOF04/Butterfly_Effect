using UnityEngine;
using System.Collections.Generic;

public class DataController : MonoBehaviour
{
    [Header("Scene References (drag the GOs here)")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private TraitManager traitManager;

    //exposes storages 
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