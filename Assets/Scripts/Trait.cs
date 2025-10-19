using UnityEngine;
using System.Collections.Generic;

/*
This is the trait object which holds the information about each unique trait that can exist as well as the effects it has
*/
[System.Serializable]
public class Trait
{
    public int id; //unique ID number
    public string displayName; //traits name
    public string description; //short description of trait 

    public Dictionary<string, float> attributeNumericalEffects = new Dictionary<string, float>(); //dictionary of effects that alter attributes (where string is the attribute and float is the change to said attribute)
    public Dictionary<string, float> statNumericalEffects = new Dictionary<string, float>(); //dictionary of effects that alter stats (where string is the stat name and float is the change to said stat)
    public List<string> behaviouralEffects; //list of additional effects
}
