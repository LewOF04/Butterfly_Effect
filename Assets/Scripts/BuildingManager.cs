using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
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
}
