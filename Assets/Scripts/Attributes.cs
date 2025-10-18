using UnityEngine;

/*
 * These are the traits of the NPC, which help outline their personality and defines how they decide to take action.
 */
[System.Serializable]
public class Attributes
{
    //all attributes range on a scale of 0-100 with 50 being considered the "average" level
    [SerializeField] private float morality { get; set; }
    [SerializeField] private float intelligence { get; set; }
    [SerializeField] private float rationality { get; set; }
    [SerializeField] private float fortitude { get; set; }
    [SerializeField] private float charisma { get; set; }
    [SerializeField] private float aesthetic_sensitivity { get; set; }
    [SerializeField] private float perception { get; set; }
    [SerializeField] private float dexterity { get; set; }
    [SerializeField] private float constitution { get; set; }
    [SerializeField] private float wisdom { get; set; }
    [SerializeField] private float strength { get; set; }
}
