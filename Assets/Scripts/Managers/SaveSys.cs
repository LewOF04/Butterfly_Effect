using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSys
{
    static string NPCDataPath = Application.persistentDataPath + "/NPCSaveData.json"; //defines the save location of the NPC Data
    static string BuildingDataPath = Application.persistentDataPath + "/BuildingSaveData.json"; //defines the save location of the Building Data
    static string RelationshipDataPath = Application.persistentDataPath + "/RelationshipSaveData.json"; //defines the save location of the Relationship Data
    static string NPCEventDataPath = Application.persistentDataPath + "/NPCEventSaveData.json"; //defines the save location of the NPC Event Data
    static string NPCHistoryTrackerDataPath = Application.persistentDataPath + "/NPCHistoryTrackerData.json"; //defines the save location for the NPC History Tracker Data
    static string BuildingEventDataPath = Application.persistentDataPath + "/BuildingEventSaveData.json"; //defines the save location for the Building Event Data
    static string BuildingHistoryTrackerDataPath = Application.persistentDataPath + "/BuildingHistoryTrackerData.json"; //defines the save location for the Building History Tracker Data

    /*
    Function to convert dictionary of NPCs currently stored in the game into a JSON for saving

    @param map - the dictionary containing all of the NPCs with their related IDs
    */
    public static void SaveAllNPCs(Dictionary<int, NPC> map)
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
    public static void SaveAllBuildings(Dictionary<int, Building> map)
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

    /*
    Function to convert dictionary of Relationships currently stored in game into a JSON for saving

    @param map - the dictionary containing all of the Relationships with their related IDs
    */
    public static void SaveRelationships(Dictionary<RelationshipKey, Relationship> map)
    {
        RelationshipDatabase db = new RelationshipDatabase();
        foreach (var kvp in map)
        {
            var relationship = kvp.Value;
            db.items.Add(relationship);
        }

        var json = JsonUtility.ToJson(db, true);
        File.WriteAllText(RelationshipDataPath, json);
        Debug.Log($"Saved {db.items.Count} Relationships to: " + RelationshipDataPath);
    }

    /*
    Function to reload all relationships from stored JSON

    @return - List of relationship objects
    */
    public static List<Relationship> LoadRelationships()
    {
        if (!File.Exists(RelationshipDataPath)) //checks whether load file exists
        {
            Debug.LogError("Save file not found: " + RelationshipDataPath);
            return null;
        }

        var json = File.ReadAllText(RelationshipDataPath); //reads the Building Data from the json
        var data = JsonUtility.FromJson<RelationshipDatabase>(json); //returns to a BuildingDatabase from the json format
        Debug.Log($"Loaded {data?.items?.Count ?? 0} NPCs from: {RelationshipDataPath}");
        return data.items; //returns the list within the database
    }

    /*
    Function to reload all NPCEvents from stored JSON

    @return - List of all NPCEvent objects
    */
    public static List<NPCEvent> LoadNPCHistory()
    {
        if (!File.Exists(NPCEventDataPath))
        {
            Debug.LogError("Save file not found: " + NPCEventDataPath);
            return null;
        }

        var json = File.ReadAllText(NPCEventDataPath);
        var data = JsonUtility.FromJson<NPCEventDatabase>(json);
        Debug.Log($"Loaded {data?.items?.Count ?? 0} NPCs from: {NPCEventDataPath}");
        return data.items;
    }

    /*
    Function to save all NPCEvents to a JSON
    */
    public static void SaveNPCHistory(Dictionary<RelationshipKey, Dictionary<NPCEventKey, NPCEvent>> map)
    {
        NPCEventDatabase db = new NPCEventDatabase();
        var keys = new List<RelationshipKey>(map.Keys);
        foreach (RelationshipKey relKey in keys)
        {
            Dictionary<NPCEventKey, NPCEvent> innerDict = map[relKey];
            List<NPCEventKey> innerKeys = new List<NPCEventKey>(innerDict.Keys);
            foreach(NPCEventKey key in innerKeys)
            {
                db.items.Add(innerDict[key]);
            }
        }

        var json = JsonUtility.ToJson(db, true);
        File.WriteAllText(NPCEventDataPath, json);
        Debug.Log($"Saved {db.items.Count} NPC events to: " + NPCEventDataPath);
    }

    /*
    Function to save NPC History Tracker information to a JSON
    */
    public static void SaveNPCHistoryTracker(NPCHistoryTracker tracker)
    {
        NPCHistoryTrackerDatabase db = new NPCHistoryTrackerDatabase(tracker.largestInt, tracker.missingInts);
        
        var json = JsonUtility.ToJson(db, true);
        File.WriteAllText(NPCHistoryTrackerDataPath, json);
        Debug.Log("Saved NPC History Tracker to: " + NPCHistoryTrackerDataPath);
    }

    /*
    Function to reload NPC Tracker Data from JSON

    @return - an NPCHistoryTrackerDatabase object
    */
    public static NPCHistoryTrackerDatabase LoadNPCHistoryTracker()
    {
        var json = File.ReadAllText(NPCHistoryTrackerDataPath);
        var data = JsonUtility.FromJson<NPCHistoryTrackerDatabase>(json);
        
        return data;
    }

    /*
    Function to save building history data
    */
    public static SaveBuildingHistory(Dictionary<BuildingRelationshipKey, Dictionary<BuildingEventKey, BuildingEvent>> map)
    {
        BuildingEventDatabase db = new BuildingEventDatabase();
        var keys = new List<BuildingEventKey>(map.Keys);
        foreach(BuildingRelationshipKey key in keys)
        {
            Dictionary<BuildingEventKey, BuildingEvent> innerDict = map[key];
            var innerKeys = new List<BuildingEventKey>(innerDict.Keys);
            foreach(BuildingEventKey innerKey in innerKeys)
            {
                db.items.Add(innerDict[innerKey]);
            }
        }

        var json = JsonUtility.ToJson(db, true);
        File.WriteAllText(BuildingEventDataPath, json);
        Debug.Log($"Saved {db.items.Count} Building events to: " + BuildingEventDataPath);
    }

    /*
    Function to save Building History Tracker information to JSON
    */
    public static SaveBuildingHistoryTracker(BuildingHistoryTracker tracker)
    {
        BuildingHistoryTrackerDatabase db = new BuildingHistoryTrackerDatabase(tracker.largestInt, tracker.missingInts);
        
        var json = JsonUtility.ToJson(db, true);
        File.WriteAllText(BuildingHistoryTrackerDataPath, json);
        Debug.Log("Saved Building History Tracker to: " + BuildingHistoryTrackerDataPath);
    }
}
