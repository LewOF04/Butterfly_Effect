using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each NPC
 */
 public class NPC : MonoBehaviour, IAgent
{
    public int id; //unique ID identifier for the NPC
    public string firstName = ""; //name of the NPC
    public string surname = "";
    public string fullName => firstName+" "+surname;
    public Attributes attributes; //the attributes of the character
    public Stats stats; //the stats of the character
    public List<int> traits; //list of ids of the traits that this NPC has
    public int spriteType; //the code of the sprite
    public int parentBuilding; //the building that the NPC lives in (-1 if non)
    public bool hasJob; //whether or not this npc has a job
    public bool isAlive; //whether this agent is alive or not

    int IAgent.id { get => id; set => id = value; }
    string IAgent.firstName { get => firstName; set => firstName = value; }
    string IAgent.surname { get => surname; set => surname = value; }
    string IAgent.fullName { get => fullName; }
    Attributes IAgent.attributes { get => attributes; set => attributes = value; }
    Stats IAgent.stats { get => stats; set => stats = value; }
    List<int> IAgent.traits { get => traits; set => traits = value; }
    int IAgent.spriteType { get => spriteType; set => spriteType = value; }
    int IAgent.parentBuilding { get => parentBuilding; set => parentBuilding = value; }
    bool IAgent.hasJob { get => hasJob; set => hasJob = value; }
    bool IAgent.isAlive { get => isAlive; set => isAlive = value; }

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
        surname = data.surname;
        firstName = data.firstName;
        attributes = data.attributes;
        stats = data.stats;
        traits = data.traits; //converts into list from array
        spriteType = data.spriteType;
        parentBuilding = data.parentBuilding;
        hasJob = data.hasJob;
        isAlive = data.isAlive;
    }

    public void Load(int inputID, string inputFirstName, string inputSurname, Attributes inputAttributes, Stats inputStats, List<int> inputTraits, int inputSpriteType, int inputParentBuilding, bool inputHasJob, bool inputIsAlive)
    {
        id = inputID;
        firstName = inputFirstName;
        surname = inputSurname;
        attributes = inputAttributes;
        stats = inputStats;
        traits = inputTraits;
        spriteType = inputSpriteType;
        parentBuilding = inputParentBuilding; 
        hasJob = inputHasJob;
        isAlive = inputIsAlive;
    }
}
