using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each Building
 */
 public class Building : MonoBehaviour
{
    public string id; //unique ID identifier for the NPC
    public string buildingName; //name of the building
    public int buildingType; //the type of building [0,1,2,3,4]
    public float condition; //the condition of the building from 0-100

    public void Load(BuildingData data)
    {
        id = data.id;
        buildingName = data.buildingName;
        buildingType = data.buildingType;
        condition = data.condition;
    }
}
