using UnityEngine;
using System.Collections.Generic;
using System;

public class RelationshipManager : MonoBehaviour
{
    private DataController dataController;

    private void Awake()
    {
        dataController = DataController.Instance;
    }
    public void GenerateRelationships(System.Random rng)
    {
        Dictionary<int, NPC> npcs = dataController.NPCStorage;

        Dictionary<RelationshipKey, Relationship> RelationshipStorage = new Dictionary<RelationshipKey, Relationship>();
        Dictionary<int, List<Relationship>> RelationshipPerNPC = new Dictionary<int, List<Relationship>>();
        List<int> npcKeys = new List<int>(npcs.Keys);
        foreach(int npcKey1 in npcKeys)
        {
            NPC npc_1 = npcs[npcKey1];
            foreach(int npcKey2 in npcKeys)
            {
                if(npcKey1 >= npcKey2) continue; //check we're not duplicating npcs or relationships
                
                NPC npc_2 = npcs[npcKey2];
                RelationshipKey key = new RelationshipKey(npcKey1, npcKey2); //calculate key of the relationship

                Relationship rel = new Relationship(key, rng.Next(0,101));
                RelationshipStorage[key] = rel;  

                //add relationship to per NPC 
                if(!RelationshipPerNPC.ContainsKey(npc_1.id)) RelationshipPerNPC[npc_1.id] = new List<Relationship>();
                if(!RelationshipPerNPC.ContainsKey(npc_2.id)) RelationshipPerNPC[npc_2.id] = new List<Relationship>();

                RelationshipPerNPC[npc_1.id].Add(rel);
                RelationshipPerNPC[npc_2.id].Add(rel);
            }
        }

        if(npcs.Count == 1 || npcs.Count == 0)
        {
            foreach(var kvp in npcs)
            {
                RelationshipPerNPC[kvp.Value.id] = new List<Relationship>();
            }
        }

        dataController.RelationshipStorage = RelationshipStorage;
        dataController.RelationshipPerNPCStorage = RelationshipPerNPC;
    }

    public void SaveRelationships()
    {
        SaveSys.SaveRelationships(dataController.RelationshipStorage);
    }

    public void LoadRelationships()
    {
        List<Relationship> relationships = SaveSys.LoadRelationships(); //gets the list of stored relationships

        Dictionary<RelationshipKey, Relationship> RelationshipStorage = new Dictionary<RelationshipKey, Relationship>(); //dictionary storing each building currently within the game 

        Dictionary<int, List<Relationship>> RelationshipPerNPC = new Dictionary<int, List<Relationship>>();

        foreach (Relationship relationship in relationships)
        {
            RelationshipStorage.Add(relationship.key, relationship); //adds the relationship to the storage

            if(!RelationshipPerNPC.ContainsKey(relationship.key.npcA)) RelationshipPerNPC[relationship.key.npcA] = new List<Relationship>();
            if(!RelationshipPerNPC.ContainsKey(relationship.key.npcB)) RelationshipPerNPC[relationship.key.npcB] = new List<Relationship>();

            //adds the relationship to storage for each individual npc
            RelationshipPerNPC[relationship.key.npcA].Add(relationship);
            RelationshipPerNPC[relationship.key.npcB].Add(relationship);
        }

        dataController.RelationshipStorage = RelationshipStorage;
        dataController.RelationshipPerNPCStorage = RelationshipPerNPC;
    }
}
 