using UnityEngine;
using System.Collections.Generic;
using System;

public class BuildingManager : MonoBehaviour
{
    const int MAX_HOUSES = 5;
    const int MIN_HOUSES = 0;
    //TODO: this number is entirely arbitrary
    public Building buildingPrefab;

    //saving and loading

    public void SaveBuildings(Dictionary<string, Building> BuildingStorage)
    {
        SaveSys.SaveAllBuildings(BuildingStorage);
    }

    public Dictionary<string, Building> LoadBuildings()
    {
        var buildings = SaveSys.LoadAllBuildings(); //gets the list of stored Buildings

        Dictionary<string, Building> BuildingStorage = new Dictionary<string, Building>(); //dictionary storing each building currently within the game 

        foreach (var buildingData in buildings)
        {
            var inst = Instantiate(buildingPrefab); //instantiates the Building
            var building = inst.GetComponent<Building>(); //gets the monoBehaviour script linked to the instantiation

            building.Load(buildingData); //loads the npc with data

            BuildingStorage.Add(building.id, building); //adds the instantiated NPC to the storage
        }

        return BuildingStorage;
    }

    public Dictionary<string, Building> generateBuildings(System.Random rng)
    {
        //number of houses in the game
        int houseNumber = rng.Next(MIN_HOUSES, MAX_HOUSES);
        Dictionary<string, Building> BuildingStorage = new Dictionary<string, Building>(); //dictionary storing each building currently within the game 

        for (int i = 0; i < houseNumber; i++)
        {
            var building = generateBuilding(i.ToString(), rng);

            BuildingStorage.Add(building.id, building); //adds the instantiated NPC to the storage
        }

        return BuildingStorage;
    }

    public Building generateBuilding(string id, System.Random rng)
    {
        string buildingName = ""; //TODO: a way to generate house names
        int buildingType = rng.Next(1, 5);
        float condition = rng.Next(0, 100);

        var inst = Instantiate(buildingPrefab); //instantiates the Building
        var building = inst.GetComponent<Building>(); //gets the monoBehaviour script linked to the instantiation

        building.Load(id, buildingName, buildingType, condition); //loads the npc with data

        return building;
    }
}
