using UnityEngine;
using System.Collections.Generic;

/*
 * The base class for each NPC
 */
[System.Serializable]
public class NPC : MonoBehaviour
{
    public string id; //unique ID identifier for the NPC
    [SerializeField] private string npcName { get; set; } //name of the NPC
    [SerializeField] private Attributes attributes { get; set; } = new Attributes(); //the attributes of the character
    [SerializeField] private Stats stats { get; set; } = new Stats(); //the stats of the character
    [SerializeField] private List<int> traits { get; set; } = new List<int>(); //list of ids of the traits that this NPC has
    [SerializeField] private int buildingID { get; set; } //the building which the NPC belongs to (-1 if no such building exists)

    //extra utility functions for traits list
    public bool containsTrait(int id)
    {
        return traits.Contains(id);
    }
    public void addTrait(int id)
    {
        traits.Add(id);
    }

    public void SaveNPC()
    {
        SaveSys.SaveNPC(this);
    }

    public void LoadPlayer()
    {
        NPCData data = SaveSys.LoadNPC();

        id = data.get(id);
        npcName = data.get(npcName);
        attributes = data.get(attributes);
        stats = data.get(stats);
        traits = data.get(traits).ToList();
        buildingID = data.get(buildingID);
    }

    /*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
