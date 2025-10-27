using UnityEngine;

/*
 * These are the stats of each NPC, which includes information about their current condition
 */
[System.Serializable]
public class Stats
{
    //all statistics range from 0-100 with 50 being considered "standard" level
    [Range(0,100)] public float condition;
    [Range(0,100)] public float nutrition;
    [Range(0,100)] public float happiness;
    [Range(0,100)] public float energy;

    //quantity of food and money owned by the NPC
    [Min(0)] public float food;
    [Min(0)] public float wealth;

}