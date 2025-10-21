using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingData
{
    public string id; //unique ID identifier for the NPC
    public string buildingName; //name of the building
    public int buildingType; //the type of building [0,1,2,3,4]
    public float condition; //the condition of the building from 0-100

    public BuildingData() { }

    public BuildingData(Building building)
    {
        id = building.id;
        buildingName = building.buildingName;
        buildingType = building.buildingType;
        condition = building.condition;
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
