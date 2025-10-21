using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectPair
{
    public string name;
    public float effect;
}

/*
This is the trait object which holds the information about each unique trait that can exist as well as the effects it has
*/
[System.Serializable]
public class TraitData
{
    public int id; //unique ID number
    public string displayName; //traits name
    public string description; //short description of trait 

    public List<EffectPair> attributeNumericalEffects = new List<EffectPair>(); //list of effects that alter attributes (where string is the attribute and float is the change to said attribute)
    public List<EffectPair> statNumericalEffects = new List<EffectPair>(); //list of effects that alter stats (where string is the stat name and float is the change to said stat)
    public List<string> behaviouralEffects; //list of additional effects
}


/*
* Wrapper for TraitData list for when storing 
*/
[System.Serializable]
public class TraitDatabase
{
    public List<TraitData> items = new List<TraitData>();
}
