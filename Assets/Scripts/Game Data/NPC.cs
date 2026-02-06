using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each NPC
 */
 public class NPC : MonoBehaviour
{
    public int id; //unique ID identifier for the NPC
    public string npcName; //name of the NPC
    public Attributes attributes; //the attributes of the character
    public Stats stats; //the stats of the character
    public List<int> traits; //list of ids of the traits that this NPC has
    public int spriteType; //the code of the sprite
    public int parentBuilding; //the building that the NPC lives in (-1 if non)
    public bool hasJob; //whether or not this npc has a job

    //action performing
    public float timeLeft = 24f; //the amount of time they have left in the day
    public ActionInfoWrapper? action; //the action the agent is in the process of performing

    //extra utility functions for traits list
    public bool ContainsTrait(int id)
    {
        return traits.Contains(id);
    }
    
    public void AddTrait(int id)
    {
        traits.Add(id);
    }

    public void Load(NPCData data)
    {
        id = data.id;
        npcName = data.npcName;
        attributes = data.attributes;
        stats = data.stats;
        traits = new List<int>(data.traits ?? System.Array.Empty<int>()); //converts into list from array
        spriteType = data.spriteType;
        parentBuilding = data.parentBuilding;
        hasJob = data.hasJob;
    }

    public void Load(int inputID, string inputNpcName, Attributes inputAttributes, Stats inputStats, List<int> inputTraits, int inputSpriteType, int inputParentBuilding, bool inputHasJob)
    {
        id = inputID;
        npcName = inputNpcName;
        attributes = inputAttributes;
        stats = inputStats;
        traits = inputTraits;
        spriteType = inputSpriteType;
        parentBuilding = inputParentBuilding; 
        hasJob = inputHasJob;
    }
}
