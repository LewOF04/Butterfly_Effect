using UnityEngine;
using System.Collections.Generic;
using System;

public class BuildingManager : MonoBehaviour
{
    int MAX_HOUSES => GameConstants.MAX_BUILDINGS;
    int MIN_HOUSES => GameConstants.MIN_BUILDINGS;

    [Header("Text Files")]
    public TextAsset buildingTypeNames;
    public TextAsset buildingNames;
    public Building buildingPrefab;
    private DataController dataController; 

    private void Awake()
    {
        dataController = DataController.Instance;
    }

    //saving and loading
    public void SaveBuildings()
    {
        SaveSys.SaveAllBuildings(dataController.BuildingStorage);
    }

    public void LoadBuildings()
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

        dataController.BuildingStorage = BuildingStorage;
    }

    public void GenerateBuildings(System.Random rng)
    {
        //number of houses in the game
        int houseNumber = rng.Next(MIN_HOUSES, MAX_HOUSES);
        Dictionary<int, Building> BuildingStorage = new Dictionary<int, Building>(); //dictionary storing each building currently within the game 

        for (int i = 0; i < houseNumber; i++)
        {
            var building = generateBuilding(i, rng);

            BuildingStorage.Add(building.id, building); //adds the instantiated NPC to the storage
        }

        dataController.BuildingStorage = BuildingStorage;
    }

    private Building generateBuilding(int id, System.Random rng)
    {
        int buildingType = rng.Next(1, 6);
        float condition = rng.Next(0, 101);

        var inst = Instantiate(buildingPrefab); //instantiates the Building
        inst.transform.SetParent(dataController.buildingContainer, false);
        var building = inst.GetComponent<Building>(); //gets the monoBehaviour script linked to the instantiation

        building.Load(id, "", buildingType, condition); //loads the building with data
        generateBuildingName(building);

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

    public Dictionary<int, BuildingData> DeepClone()
    {
        Dictionary<int, Building> buildingStorage = dataController.BuildingStorage;

        Dictionary<int, BuildingData> outputDict = new Dictionary<int, BuildingData>();

        List<int> buildingKeys = new List<int>(buildingStorage.Keys);
        foreach(int buildingKey in buildingKeys)
        {
            outputDict[buildingKey] = new BuildingData(buildingStorage[buildingKey]);
        }

        return outputDict;
    }

    private void generateBuildingName(IBuilding building)
    {
        int seed = HashCode.Combine(
            building.id,
            Mathf.RoundToInt(building.condition * 100f)
        );
        System.Random rng = new System.Random(seed); //weight based on building info
        List<string> typeNames = new List<string>(buildingTypeNames.text.Split('\n'));
        List<string> names = new List<string>(buildingNames.text.Split('\n'));

        building.buildingName = GetRandomString(names, rng) + " " + GetRandomString(typeNames, rng);
    }

    private string GetRandomString(List<string> strings, System.Random rng)
    {
        int index = rng.Next(0, strings.Count);
        return strings[index].Trim();
    }
}
