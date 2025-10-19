using UnityEngine;

/*
 * These are the traits of the NPC, which help outline their personality and defines how they decide to take action.
 */
[System.Serializable]
public class Attributes
{
    //all attributes range on a scale of 0-100 with 50 being considered the "average" level
    public float morality;
    public float intelligence;
    public float rationality;
    public float fortitude;
    public float charisma;
    public float aesthetic_sensitivity;
    public float perception;
    public float dexterity;
    public float constitution;
    public float wisdom;
    public float strength;
}
