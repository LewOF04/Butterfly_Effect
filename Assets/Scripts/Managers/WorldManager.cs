using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class WorldManager : MonoBehaviour, IWorldData
{
    public float gameTime; //the time of the game in hours
    public float costPerFood = 1f;
    [Range(0,100)] public float chaosModifier;
    public float[] costPerBuilding;
    private DataController dataController;

    float IWorldData.gameTime { get => gameTime; set => gameTime = value; }
    float IWorldData.costPerFood { get => costPerFood; set => costPerFood = value; }
    float IWorldData.chaosModifier { get => chaosModifier; set => chaosModifier = value; }
    float[] IWorldData.costPerBuilding { get => costPerBuilding; set => costPerBuilding = value; }

    public WorldManager()
    {
        gameTime = 0f;
        chaosModifier = 0f;
    }

    public void Awake()
    {
        dataController = DataController.Instance;
    }
    public void LoadWorldData()
    {
        WorldManager saved = SaveSys.LoadWorldData();
        gameTime = saved.gameTime;
    }

    public void SaveWorldData()
    {
        SaveSys.SaveWorldData(this);
    }

    public void GenerateWorldData(System.Random rng)
    {
        gameTime = 0f;
    }

    public WorldData DeepClone()
    {
        return new WorldData(gameTime, costPerFood, chaosModifier, costPerBuilding);
    }
}