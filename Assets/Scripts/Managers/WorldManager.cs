using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class WorldManager : MonoBehaviour
{
    public float gameTime;
    public float costPerNutrition;
    public float[] costPerBuilding;
    private DataController dataController;

    public WorldManager(float time)
    {
        gameTime = time;
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
        gameTime = 0.0f;
    }
}