using UnityEngine;
using System.Collections.Generic;
using System;

public class RelationshipManager : MonoBehaviour
{
    public Dictionary<RelationshipKey, Relationship> generateRelationships(System.Random rng, Dictionary<int, NPC> npcs)
    {
        Dictionary<RelationshipKey, Relationship> RelationshipStorage = new Dictionary<RelationshipKey, Relationship>();
        foreach(NPC npc_1 in npcs)
        {
            foreach(NPC npc_2 in npcs)
            {
                //check we're not duplicating npcs
                if(npc_1.id == npc_2.id)
                {
                    continue;
                }

                RelationshipKey key = new RelationshipKey(npc_1, npc_2); //calculate key of the relationship

                if(RelationshipStorage[key] == null) //check that relationship doesn't already exist
                {
                  Relationship rel = new Relationship(key, rng.Next(0,100));
                  RelationshipStorage[key] = rel;  
                }
            }
        }
    }

    public void SaveRelationships(Dictionary<RelationshipKey, Relationship> relationshipStorage)
    {
        SaveSys.SaveRelationships(relationshipStorage);
    }

    public Dictionary<RelationshipKey, Relationship> LoadRelationships()
    {
        List<Relationship> relationships = SaveSys.LoadRelationships(); //gets the list of stored relationships

        Dictionary<RelationshipKey, Relationship> RelationshipStorage = new Dictionary<RelationshipKey, Relationship>(); //dictionary storing each building currently within the game 

        Dictionary<int, List<Relationship>> RelationshipsPerNPC = new Dictionary<int, List<Relationship>>();

        foreach (Relationship relationship in relationships)
        {
            RelationshipStorage.Add(relationship.key, relationship); //adds the relationship to the storage

            //adds the relationship to storage for each individual npc
            RelationshipByNPC[relationship.key.npcA].Add(relationship);
            RelationshipByNPC[relationship.key.npcB].Add(relationship);
        }

        return RelationshipStorage, RelationshipByNPC; 
    }
}
 