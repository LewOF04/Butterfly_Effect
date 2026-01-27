using System;

[System.Serializable]
public struct BuildingEventKey : IEquatable<BuildingEventKey>
{
    public int npc; //ID of the first npc
    public int building; //ID of the second npc
    public int eventNum; //the number of the amount of events between this NPC and building 
    public BuildingEventKey(int npcID, int buildingID, int num)
    {
        npc = npcID;
        building = buildingID;
        eventNum = num;
    }

    public bool Equals(BuildingEventKey other) => npc == other.npc && building == other.building && eventNum == other.eventNum; //check if two event keys are the same
    public override int GetHashCode() => HashCode.Combine(npc, building, eventNum); //produce a hash code for the event
}