using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSys
{
    string NPCDataPath = Application.persistentDataPath + "/NPCSaveData.json";
    public static void SaveNPC(NPC npc)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        fileStream stream = new fileStream(NPCDataPath, FileMode.Create);

        NPCData data = new NPCData(npc);

        formatter.Serialize(stream, data);
        stream.close();
    }
    
    public static NPCData LoadNPC()
    {
        if (File.Exists(NPCDataPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(NPCDataPath, FileMde.Open);

            NPCData data = formatter.Deserialize(stream) as NPCData;
            stream.close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in : " + NPCDataPath);
            return null;
        }
    }
}
