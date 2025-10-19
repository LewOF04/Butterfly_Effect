using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public Dictionary<string, NPC> NPCStorage = new Dictionary<string, NPC>(); //dictionary storing each NPC currently within the game 
    public NPC npcPrefab;

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
            var inst = Instantiate(npcPrefab); //instantiates the NPC
            var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

            npc.Load(npcData); //loads the npc with data

            NPCStorage.Add(npc.id, npc); //adds the instantiated NPC to the storage
        }
    }
}
