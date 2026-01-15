using System;

[System.Serializable]
public struct NPCEventKey : IEquatable<NPCEventKey>
{
    public int npcA; //ID of the first npc
    public int npcB; //ID of the second npc
    public int eventNum; //the number of the amount of events between these two NPCs 

    public NPCEventKey(int id1, int id2, int num)
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
        eventNum = num;
    }

    public bool Equals(NPCEventKey other) => npcA == other.npcA && npcB == other.npcB && eventNum == other.eventNum; //check if two event keys are the same
    public override int GetHashCode() => HashCode.Combine(npcA, npcB, eventNum); //produce a hash code for the event
}