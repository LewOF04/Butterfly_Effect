using System;

[System.Serializable]
public struct RelationshipKey : IEquatable<RelationshipKey>
{
    public int npcA; //ID of the first npc
    public int npcB; //ID of the second npc

    public RelationshipKey(int id1, int id2)
    {
        //ensure that no matter which way it is entered, it is the same
        if (id1 < id2)
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