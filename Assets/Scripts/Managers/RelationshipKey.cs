using System;

[System.Serializable]
public struct RelationshipKey : IEquatable<RelationshipKey>
{
    public int npcA; //ID of the first npc
    public int npcB; //ID of the second npc

    public RelationshipKey(int id1, int id2)
    {
        //ensure that no matter which way it is entered, it is the same
        if(id1 == -1 && id2 == -1)
        {
            Debug.LogError("Both ids cannot be -1.");
            return null;
        }else if(id1 == -1){
            npcA = id2;
            npcB = -1;

        } else if(id2 == -1){
            npcA = id1;
            npcB = -1;

        } else if (id1 < id2)
        {
            npcA = id1;
            npcB = id2;
        }
        else
        {
            npcA = id2;
            npcB = id1;
        }
    }

    public bool Equals(RelationshipKey other) => npcA == other.npcA && npcB == other.npcB; //check if two relationship keys are the same
    public override int GetHashCode() => HashCode.Combine(npcA, npcB); //produce a hash code for the two IDs
}