using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class WorldManager : MonoBehaviour
{
    public float gameTime; //the time of the game in hours
    public float costPerFood = 1f;
    [Range(0,100)] public float chaosModifier;
    public float[] costPerBuilding;
    private DataController dataController;

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
}