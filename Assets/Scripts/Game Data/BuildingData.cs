using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingData : IBuilding
{
    public int id; //unique ID identifier for the Building
    public string buildingName; //name of the building
    [Range(1,5)] public int buildingType; //the type of building [0,1,2,3,4]
    [Range(0, 100)] public float condition; //the condition of the building from 0-100
    public List<int> inhabitants = new List<int>(); //string of inhabitant NPCs

    int IBuilding.id { get => id; set => id = value; }
    string IBuilding.buildingName { get => buildingName; set => buildingName = value; }
    int IBuilding.buildingType { get => buildingType; set => buildingType = value; }
    float IBuilding.condition { get => condition; set => condition = value; }
    List<int> IBuilding.inhabitants { get => inhabitants; set => inhabitants = value; }

    public BuildingData() { }

    public BuildingData(Building building)
    {
        id = building.id;
        buildingName = building.buildingName;
        buildingType = building.buildingType;
        condition = building.condition;
        inhabitants = new List<int>(building.inhabitants);
    }
}

/*
Wrapper for BuildingData list that can be used as a stepping stone when serializing and de-serializing
*/
[System.Serializable]
public class BuildingDatabase
{
    public List<BuildingData> items = new List<BuildingData>();
}
 