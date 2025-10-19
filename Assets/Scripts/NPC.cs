using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each NPC
 */
 public class NPC : MonoBehaviour
{
    public string id; //unique ID identifier for the NPC
    public string npcName; //name of the NPC
    public Attributes attributes = new Attributes(); //the attributes of the character
    public Stats stats = new Stats(); //the stats of the character
    public List<int> traits = new List<int>(); //list of ids of the traits that this NPC has
    public int buildingID; //the building which the NPC belongs to (-1 if no such building exists)

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
        buildingID = data.buildingID;
    }
}
