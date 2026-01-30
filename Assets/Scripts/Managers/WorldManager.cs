using UnityEngine;
using Systems.Collections.Generic;
using System.Random;

[System.Serializable]
public class WorldManager : MonoBehaviour
{
    public float gameTime;
    private DataController dataController;

    public WorldManager(float time)
    {
        gameTime = time;
    }

    public Awake()
    {
        dataController = DataController.Instance;
    }
    public LoadWorldData()
    {
        WorldManager saved = SaveSys.LoadWorldData();
        gameTime = saved.gameTime;
    }

    public SaveWorldData()
    {
        SaveSys.SaveWorldData();
    }

    public GenerateWorldData(System.Random rng)
    {
        gameTime = 0.0f;
    }
}