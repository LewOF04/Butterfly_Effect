using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCData : IAgent
{
    public int id; //unique ID identifier for the NPC
    public string firstName; //name of the NPC
    public string surname;
    public string fullName;
    public Attributes attributes; //the attributes of the character
    public Stats stats; //the stats of the character
    public List<int> traits; //list of ids of the traits that this NPC has
    public int spriteType; //the code of the sprite
    public int parentBuilding;
    public bool hasJob;
    public bool isAlive;

    int IAgent.id { get => id; set => id = value; }
    string IAgent.firstName { get => firstName; set => firstName = value; }
    string IAgent.surname { get => surname; set => surname = value; }
    string IAgent.fullName { get => fullName; set => fullName = value; }
    Attributes IAgent.attributes { get => attributes; set => attributes = value; }
    Stats IAgent.stats { get => stats; set => stats = value; }
    List<int> IAgent.traits { get => traits; set => traits = value; }
    int IAgent.spriteType { get => spriteType; set => spriteType = value; }
    int IAgent.parentBuilding { get => parentBuilding; set => parentBuilding = value; }
    bool IAgent.hasJob { get => hasJob; set => hasJob = value; }
    bool IAgent.isAlive { get => isAlive; set => isAlive = value; }

    public NPCData() { }

    public NPCData(NPC npc)
    {
        id = npc.id;
        firstName = npc.firstName;
        surname = npc.surname;
        fullName = firstName + " " + surname;
        attributes = npc.attributes.DeepClone();
        stats = npc.stats.DeepClone();
        traits = new List<int>(npc.traits); 
        spriteType = npc.spriteType;
        parentBuilding = npc.parentBuilding;
        hasJob = npc.hasJob;
        isAlive = npc.isAlive;
    }
}

/*
Wrapper for NPCData list that can be used as a stepping stone when serializing and de-serializing
*/
[System.Serializable]
public class NPCDatabase
{
    public List<NPCData> items = new List<NPCData>();
}
