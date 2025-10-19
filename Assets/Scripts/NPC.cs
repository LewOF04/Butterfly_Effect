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


    //TODO: These functions probably need to be done in a data structure that can hold all of the various NPCs (probably a hashmap)
    //saving and loading
    public void SaveNPC()
    {
        SaveSys.SaveNPC(this); //passes the NPCs data into the save system
    }

    public void LoadNPC()
    {
        NPCData data = SaveSys.LoadNPC(); //TODO: Currently this loads a singular NPC, need to be able to iterate through NPC loads

        id = data.id;
        npcName = data.npcName;
        attributes = data.attributes;
        stats = data.stats;
        traits = data.traits.ToList(); //converts the NPCData (which is stored as an array) into a list to be stored in the NPC
        buildingID = data.buildingID;

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
