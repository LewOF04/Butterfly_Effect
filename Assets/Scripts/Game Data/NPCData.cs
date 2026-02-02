using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCData
{
    public int id;//unique ID identifier for the NPC
    public string npcName;//name of the NPC
    public Attributes attributes; //the attributes of the character
    public Stats stats; //the stats of the character
    public int[] traits; //list of ids of the traits that this NPC has
    public int spriteType; //the code of the sprite
    public int parentBuilding;
    public bool hasJob;

    public NPCData() { }

    public NPCData(NPC npc)
    {
        id = npc.id;
        npcName = npc.npcName;
        attributes = npc.attributes;
        stats = npc.stats;
        traits = npc.traits.ToArray(); //converts into array 
        spriteType = npc.spriteType;
        parentBuilding = npc.parentBuilding;
        hasJob = npc.hasJob;
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
