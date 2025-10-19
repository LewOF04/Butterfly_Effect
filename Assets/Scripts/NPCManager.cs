using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public Dictionary<string, NPC> NPCStorage = new Dictionary<string, NPC>(); //dictionary storing each NPC currently within the game 

    //saving and loading

     public void SaveNPCs()
    {
        SaveSys.SaveAllNPCs(NPCStorage);
    }

    public void LoadNPCs()
    {
        var npcs = SaveSys.LoadAllNPCs(); //gets the list of stored NPCs
        if (npcs == null) return;

        foreach (var npcData in npcs)
        {
            NPC npc = new NPC(); //creates a new npc
            npc.Load(npcData); //loads the npc with data

            NPCStorage.Add(npc.id, npc); //adds the npc to the storage dictionary
        }
    }
}
