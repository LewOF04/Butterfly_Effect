using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class WorldData : IWorldData
{
    public float gameTime; //the time of the game in hours
    public float costPerFood = 1f;
    [Range(0,100)] public float chaosModifier;
    public float[] costPerBuilding;

    float IWorldData.gameTime { get => gameTime; set => gameTime = value; }
    float IWorldData.costPerFood { get => costPerFood; set => costPerFood = value; }
    float IWorldData.chaosModifier { get => chaosModifier; set => chaosModifier = value; }
    float[] IWorldData.costPerBuilding { get => costPerBuilding; set => costPerBuilding = value; }

    public WorldData(float inputTime, float foodCostInput, float chaosInput, float[] buildingCostInput)
    {
        gameTime = inputTime;
        costPerFood = foodCostInput;
        chaosModifier = chaosInput;
        costPerBuilding = new float[buildingCostInput.Length];
        Array.Copy(buildingCostInput, costPerBuilding, buildingCostInput.Length);
    }
}