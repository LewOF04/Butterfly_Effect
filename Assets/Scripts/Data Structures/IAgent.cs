using System.Collections.Generic;

public interface IAgent
{
    int id { get; set; }    //unique ID identifier for the agent
    string firstName { get; set; }    //first name of the agent
    string surname { get; set; }    //last name of the agent
    string fullName { get; set; }
    Attributes attributes { get; set; } //the attributes of the character
    Stats stats { get; set; } //the stats of the character
    List<int> traits { get; set; }  //list of ids of the traits that this agent has
    int spriteType { get; set; } //the code of the sprite
    int parentBuilding { get; set; } //the building that the agent lives in (-1 if non)
    bool hasJob { get; set; }  //whether or not this agent has a job
    bool isAlive { get; set; } //whether or not the agent is alive
}