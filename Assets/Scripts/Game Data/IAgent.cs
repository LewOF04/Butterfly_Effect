using System.Collections.Generic;

public interface IAgent
{
    int id { get; set; }    //unique ID identifier for the NPC
    string npcName { get; set; }    //name of the NPC
    Attributes attributes { get; set; } //the attributes of the character
    Stats stats { get; set; } //the stats of the character
    List<int> traits { get; set; }  //list of ids of the traits that this NPC has
    int spriteType { get; set; } //the code of the sprite
    int parentBuilding { get; set; } //the building that the NPC lives in (-1 if non)
    bool hasJob { get; set; }  //whether or not this npc has a job
    float timeLeft { get; set; } 
}