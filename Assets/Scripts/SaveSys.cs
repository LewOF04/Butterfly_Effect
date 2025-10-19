using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSys
{
    string NPCDataPath = Application.persistentDataPath + "/NPCSaveData.json"; //defines the save location of the NPC Data
    public static void SaveNPC(NPC npc)
    {

        NPCData data = new NPCData(npc); //turns the NPC's data into a standard object so that it can be serialized

        //convert data to JSON
        string json = JsonUtility.ToJson(data, true);

        //write JSON to file
        File.WriteAllText(NPCDataPath, json);

        Debug.Log("Saved NPC data to: " + NPCDataPath);
    }
    
    public static NPCData LoadNPC()
    {
        if (File.Exists(NPCDataPath))
        {
            //read JSON from file
            string json = File.ReadAllText(NPCDataPath);

            //convert JSON back to NPCData object
            NPCData data = JsonUtility.FromJson<NPCData>(json);

            Debug.Log("Loaded NPC data from: " + NPCDataPath);
            return data;
        }
        else
        {
            Debug.LogError("Save file not found in: " + NPCDataPath);
            return null;
        }
    }
}
