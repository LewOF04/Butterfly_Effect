using UnityEngine;
using System.Collections.Generic;
using System;

public class BuildingManager : MonoBehaviour
{
    const int MAX_HOUSES = 10;
    const int MIN_HOUSES = 1;
    //TODO: this number is entirely arbitrary
    public Building buildingPrefab;
    private DataController dataController; 

    private void Awake()
    {
        dataController = DataController.Instance;
    }

    //saving and loading
    public void SaveBuildings(Dictionary<int, Building> BuildingStorage)
    {
        SaveSys.SaveAllBuildings(BuildingStorage);
    }

    public Dictionary<int, Building> LoadBuildings()
    {
        var buildings = SaveSys.LoadAllBuildings(); //gets the list of stored Buildings

        Dictionary<int, Building> BuildingStorage = new Dictionary<int, Building>(); //dictionary storing each building currently within the game 

        foreach (BuildingData buildingData in buildings)
        {
            var inst = Instantiate(buildingPrefab); //instantiates the Building
            inst.transform.SetParent(dataController.buildingContainer, false);
            var building = inst.GetComponent<Building>(); //gets the monoBehaviour script linked to the instantiation

            building.Load(buildingData); //loads the npc with data

            BuildingStorage.Add(building.id, building); //adds the instantiated NPC to the storage
        }

        return BuildingStorage;
    }

    public Dictionary<int, Building> generateBuildings(System.Random rng)
    {
        //number of houses in the game
        int houseNumber = rng.Next(MIN_HOUSES, MAX_HOUSES);
        Dictionary<int, Building> BuildingStorage = new Dictionary<int, Building>(); //dictionary storing each building currently within the game 

        for (int i = 0; i < houseNumber; i++)
        {
            var building = generateBuilding(i, rng);

            BuildingStorage.Add(building.id, building); //adds the instantiated NPC to the storage
        }

        return BuildingStorage;
    }

    public Building generateBuilding(int id, System.Random rng)
    {
        string buildingName = ""; //TODO: a way to generate house names
        int buildingType = rng.Next(1, 5);
        float condition = rng.Next(0, 100);

        var inst = Instantiate(buildingPrefab); //instantiates the Building
        inst.transform.SetParent(dataController.buildingContainer, false);
        var building = inst.GetComponent<Building>(); //gets the monoBehaviour script linked to the instantiation

        building.Load(id, buildingName, buildingType, condition); //loads the building with data

        return building;
    }

    public void generateSingleBuilding()
    {
        System.Random rng = new System.Random();

        //gets the lowest missing key
        int id = -1;
        int lastKey = -1;
        foreach (int key in dataController.BuildingStorage.Keys)
        {
            //checks if there are any gaps in the key id list
            if (key - 1 != lastKey)
            {
                id = key - 1;
                break;
            }
            lastKey = key;
        }

        //if no id has been set
        if (id == -1)
        {
            id = dataController.BuildingStorage.Count; //the new id is the next key     
        }

        var building = generateBuilding(id, rng);
        dataController.BuildingStorage.Add(building.id, building);
    }
}
