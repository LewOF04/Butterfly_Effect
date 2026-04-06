using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class WorldData : IWorldData
{
    public float gameTime; //the time of the game in hours
    public float costPerFood = 1f;
    public float chaosModifier => GameConstants.CHAOS_LEVEL;
    public float[] costPerBuilding;

    float IWorldData.gameTime { get => gameTime; set => gameTime = value; }
    float IWorldData.costPerFood { get => costPerFood; set => costPerFood = value; }
    float IWorldData.chaosModifier { get => chaosModifier; }
    float[] IWorldData.costPerBuilding { get => costPerBuilding; set => costPerBuilding = value; }

    public WorldData(float inputTime, float foodCostInput, float _, float[] buildingCostInput)
    {
        gameTime = inputTime;
        costPerFood = foodCostInput;
        costPerBuilding = new float[buildingCostInput.Length];
        Array.Copy(buildingCostInput, costPerBuilding, buildingCostInput.Length);
    }
}