using System;
using UnityEngine;

[System.Serializable]
public struct BuildingRelationshipKey : IEquatable<BuildingRelationshipKey>
{
    public int npc; //ID of the npc (or -1 if no npc)
    public int building; //ID of the building

    public BuildingRelationshipKey(int buildingID, int npcID)
    {
        //ensure that no matter which way it is entered, it is the same
        if(npcID == -1 && buildingID == -1)
        {
            Debug.LogError("Both ids cannot be -1.");
            npc = -1;
            building = -1;
        }
        else
        {
            npc = npcID;
            building = buildingID;
        }
    }

    public bool Equals(BuildingRelationshipKey other) => npc == other.npc && building == other.building; //check if two relationship keys are the same
    public override int GetHashCode() => HashCode.Combine(npc, building); //produce a hash code for the two IDs
}