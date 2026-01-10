using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each Building
 */
 public class Building : MonoBehaviour
{
    public int id; //unique ID identifier for the building
    public string buildingName; //name of the building
    [Range(1,5)] public int buildingType; //the type of building [1,2,3,4,5]
    [Range(0, 100)] public float condition; //the condition of the building from 0-100
    public List<int> inhabitants; //string of inhabitant NPCs

    //load the object with a building Data object
    public void Load(BuildingData data)
    {
        id = data.id;
        buildingName = data.buildingName;
        buildingType = data.buildingType;
        condition = data.condition;
        inhabitants = data.inhabitants;
    }

    //load the object with specified parameters
    public void Load(int inputID, string inputBuildingName, int inputBuildingType, float inputCondition)
    {
        id = inputID;
        buildingName = inputBuildingName;
        buildingType = inputBuildingType;
        condition = inputCondition;
        inhabitants = new List<int>();
    }
}
  