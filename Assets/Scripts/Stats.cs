using UnityEngine;

/*
 * These are the stats of each NPC, which includes information about their current condition
 */
[System.Serializable]
public class Stats
{
    //all statistics range from 0-100 with 50 being considered "standard" level
    public float condition;
    public float nutrition;
    public float happiness;
    public float energy;

    //quantity of food and money owned by the NPC
    public float food;
    public float wealth;

}