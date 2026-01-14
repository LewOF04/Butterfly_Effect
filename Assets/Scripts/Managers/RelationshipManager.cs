using UnityEngine;
using System.Collections.Generic;
using System;

public class RelationshipManager : MonoBehaviour
{
    public RelationshipWrapper generateRelationships(System.Random rng, Dictionary<int, NPC> npcs)
    {
        Dictionary<RelationshipKey, Relationship> RelationshipStorage = new Dictionary<RelationshipKey, Relationship>();
        Dictionary<int, List<Relationship>> RelationshipPerNPC = new Dictionary<int, List<Relationship>>();
        foreach(var kvp_1 in npcs)
        {
            NPC npc_1 = kvp_1.Value;
            foreach(var kvp_2 in npcs)
            {
                NPC npc_2 = kvp_2.Value;
                //check we're not duplicating npcs or relationships
                if(npc_1.id >= npc_2.id)
                {
                    continue;
                }

                RelationshipKey key = new RelationshipKey(npc_1.id, npc_2.id); //calculate key of the relationship

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
        return new RelationshipWrapper(RelationshipStorage, RelationshipPerNPC);
    }

    public void SaveRelationships(Dictionary<RelationshipKey, Relationship> relationshipStorage)
    {
        SaveSys.SaveRelationships(relationshipStorage);
    }

    public RelationshipWrapper LoadRelationships()
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

        return new RelationshipWrapper(RelationshipStorage, RelationshipPerNPC); 
    }
}

public class RelationshipWrapper
{
    public Dictionary<RelationshipKey, Relationship> RelationshipStorage;
    public Dictionary<int, List<Relationship>> RelationshipPerNPC;
    public RelationshipWrapper(Dictionary<RelationshipKey, Relationship> relStorage, Dictionary<int, List<Relationship>> relPerNPC)
    {
        RelationshipStorage = relStorage;
        RelationshipPerNPC = relPerNPC;
    }
}
 