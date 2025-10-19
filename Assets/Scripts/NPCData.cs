using UnityEngine;

[System.Serializable]
public class NPCData
{
    public string id { get; set; } //unique ID identifier for the NPC
    public string npcName { get; set; }//name of the NPC
    public Attributes attributes { get; set; } //the attributes of the character
    public Stats stats { get; set; } //the stats of the character
    public int[] traits { get; set; } //list of ids of the traits that this NPC has
    public int buildingID { get; set; } //the building which the NPC belongs to (-1 if no such building exists)

    public NPCData(NPCData npc)
    {
        id = npc.get(id);
        npcName = npc.get(npcName);
        attributes = npc.get(attributes);
        stats = npc.get(stats);
        buildingID = npc.get(buildingID);

        traits = npc.get(traits).ToArray(); //npc traits converted into an array so that it can be serialized
    }
}
