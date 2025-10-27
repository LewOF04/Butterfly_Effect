using UnityEngine;

/*
 * These are the traits of the NPC, which help outline their personality and defines how they decide to take action.
 */
[System.Serializable]
public class Attributes
{
    //all attributes range on a scale of 0-100 with 50 being considered the "average" level
    [Range(0,100)] public float morality;
    [Range(0,100)] public float intelligence;
    [Range(0,100)] public float rationality;
    [Range(0,100)] public float fortitude;
    [Range(0,100)] public float charisma;
    [Range(0,100)] public float aesthetic_sensitivity;
    [Range(0,100)] public float perception;
    [Range(0,100)] public float dexterity;
    [Range(0,100)] public float constitution;
    [Range(0,100)] public float wisdom;
    [Range(0,100)] public float strength;
}
