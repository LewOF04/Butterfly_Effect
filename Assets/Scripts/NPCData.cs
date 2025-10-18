using UnityEngine;

[System.Serializable]
public class NPCData
{
    private string id { get; set; } //unique ID identifier for the NPC
    private string npcName { get; set; }//name of the NPC
    private Attributes attributes { get; set; } //the attributes of the character
    private Stats stats { get; set; } //the stats of the character
    private int[] traits { get; set; } //list of ids of the traits that this NPC has
    private int buildingID { get; set; } //the building which the NPC belongs to (-1 if no such building exists)

    public NPCData(NPCData npc)
    {
        id = npc.get(id);
        npcName = npc.get(npcName);
        attributes = npc.get(attributes);
        stats = npc.get(stats);
        buildingID = npc.get(buildingID);

        traits = npc.get(traits).ToArray();
    }
}
