using UnityEngine;

/*
 * These are the stats of each NPC, which includes information about their current condition
 */
[System.Serializable]
public class Stats
{
    //all statistics range from 0-100 with 50 being considered "standard" level
    [SerializeField] private float condition { get; set; }
    [SerializeField] private float nutrition { get; set; }
    [SerializeField] private float happiness { get; set; }
    [SerializeField] private float energy { get; set; }

    //quantity of food and money owned by the NPC
    [SerializeField] private float food { get; set; }
    [SerializeField] private float wealth { get; set; }

}