using System.Collections.Generic;

public interface IWorldData
{
    float gameTime { get; set; } //the time of the game in hours
    float costPerFood { get; set; }
    float chaosModifier { get; }
    float[] costPerBuilding { get; set; }
}