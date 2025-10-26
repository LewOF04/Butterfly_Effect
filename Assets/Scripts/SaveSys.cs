using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSys
{
    static string NPCDataPath = Application.persistentDataPath + "/NPCSaveData.json"; //defines the save location of the NPC Data
    static string BuildingDataPath = Application.persistentDataPath + "/BuildingSaveData.json"; //defines the save location of the Building Data

    /*
    Function to convert dictionary of NPCs currently stored in the game into a JSON for saving

    @param map - the dictionary containing all of the NPCs with their related IDs
    */
    public static void SaveAllNPCs(Dictionary<string, NPC> map)
    {
        NPCDatabase db = new NPCDatabase(); //create database to store all of the NPCs for serialization
        foreach (var kvp in map) //iterates over all values within the dictionary
        {
            var npc = kvp.Value; //retrieves the NPC from the dictionary
            db.items.Add(new NPCData(npc)); //add the npc to the npcs list
        }

        var json = JsonUtility.ToJson(db, true); //turns the npcs list into json format
        File.WriteAllText(NPCDataPath, json); //writes to the NPC data json file
        Debug.Log($"Saved {db.items.Count} NPCs to: " + NPCDataPath);
    }

    /*
    Function to reloard all NPC data from the NPC save file

    @return - list of NPCData from the JSON file
    */
    public static List<NPCData> LoadAllNPCs()
    {
        if (!File.Exists(NPCDataPath)) //checks whether load file exists
        {
            Debug.LogError("Save file not found: " + NPCDataPath);
            return null;
        }

        var json = File.ReadAllText(NPCDataPath); //reads the NPCDatabase from the json
        var data = JsonUtility.FromJson<NPCDatabase>(json); //returns to a NPCDatabase from the json format
        Debug.Log($"Loaded {data?.items?.Count ?? 0} NPCs from: {NPCDataPath}");
        return data.items; //returns the list within the database
    }

    /*
    Function to reload all Building data from the Building save file

    @return - list of BuildingData from the JSON file
    */
    public static List<BuildingData> LoadAllBuildings()
    {
        if (!File.Exists(BuildingDataPath)) //checks whether load file exists
        {
            Debug.LogError("Save file not found: " + BuildingDataPath);
            return null;
        }

        var json = File.ReadAllText(BuildingDataPath); //reads the Building Data from the json
        var data = JsonUtility.FromJson<BuildingDatabase>(json); //returns to a BuildingDatabase from the json format
        Debug.Log($"Loaded {data?.items?.Count ?? 0} NPCs from: {BuildingDataPath}");
        return data.items; //returns the list within the database
    }
    
    /*
    Function to convert dictionary of NPCs currently stored in the game into a JSON for saving

    @param map - the dictionary containing all of the NPCs with their related IDs
    */
    public static void SaveAllBuildings(Dictionary<string, Building> map)
    {
        BuildingDatabase db = new BuildingDatabase(); //create database to store all of the Building for serialization
        foreach (var kvp in map) //iterates over all values within the dictionary
        {
            var building = kvp.Value; //retrieves the building from the dictionary
            db.items.Add(new BuildingData(building)); //add the building to the buildings list
        }

        var json = JsonUtility.ToJson(db, true); //turns the npcs list into json format
        File.WriteAllText(BuildingDataPath, json); //writes to the Building data json file
        Debug.Log($"Saved {db.items.Count} Buildings to: " + BuildingDataPath);
    }
}
