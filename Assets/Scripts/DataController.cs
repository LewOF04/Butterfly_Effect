using UnityEngine;
using System.Collections.Generic;
using System;

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
    public Dictionary<string, List<string>> NPCBuildingLinks;

    private void LoadFromMemory()
    {
        //loads traits first as NPCs are reliant on their existence
        TraitStorage = traitManager ? traitManager.LoadTraits() : new Dictionary<int, TraitData>();

        NPCStorage = npcManager ? (npcManager.LoadNPCs() ?? new Dictionary<string, NPC>())
                                        : new Dictionary<string, NPC>();

        BuildingStorage = buildingManager ? (buildingManager.LoadBuildings() ?? new Dictionary<string, Building>())
                                        : new Dictionary<string, Building>();

        foreach (var (id, building) in BuildingStorage)
        {
            NPCBuildingLinks.Add(id, building.inhabitants);
        }
    }

    private void seedGenerate(int seed)
    {
        System.Random rng = new System.Random(seed);
        TraitStorage = traitManager ? traitManager.LoadTraits() : new Dictionary<int, TraitData>(); //loads traits from memory

        BuildingStorage = buildingManager.generateBuildings(rng);
        NPCStorage = npcManager.generateNPCs(rng, TraitStorage.Count);
        //linkNPCsBuidlings(); //TODO: implement this
    }
}