using UnityEngine;
using System.Collections.Generic;
using System;

public class NPCManager : MonoBehaviour
{
    public NPC npcPrefab;
    private DataController dataController;

    const int MIN_NPCS = 1;
    const int MAX_NPCS = 10;
    //TODO: these numbers are entirely arbitrary

    private void Awake()
    {
        dataController = DataController.Instance;
    }

    //saving and loading
    public void SaveNPCs()
    {
        SaveSys.SaveAllNPCs(dataController.NPCStorage);
    }

    public void LoadNPCs()
    {
        var npcs = SaveSys.LoadAllNPCs(); //gets the list of stored NPCs

        Dictionary<int, NPC> NPCStorage = new Dictionary<int, NPC>(); //dictionary storing each NPC currently within the game 

        foreach (var npcData in npcs)
        {
            var inst = Instantiate(npcPrefab); //instantiates the NPC
            inst.transform.SetParent(dataController.npcContainer, false);
            var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

            npc.Load(npcData); //loads the npc with data

            NPCStorage.Add(npc.id, npc); //adds the instantiated NPC to the storage
        }

        dataController.NPCStorage = NPCStorage;
    }

    public void GenerateNPCs(System.Random rng, int maxTrait)
    {
        int npcNumber = rng.Next(MIN_NPCS, MAX_NPCS+1);

        Dictionary<int, NPC> NPCStorage = new Dictionary<int, NPC>(); //dictionary storing each NPC currently within the game 

        for (int i = 0; i < npcNumber; i++)
        {
            NPC npc = generateNPC(i, maxTrait, rng);
            NPCStorage.Add(npc.id, npc);
        }

        dataController.NPCStorage = NPCStorage;
    }

    private NPC generateNPC(int inputID, int maxTrait, System.Random rng)
    {
        int id = inputID;
        string npcName = ""; //TODO: create a way to give NPCs unique names
        Attributes attributes = new Attributes(); //the attributes of the character
        Stats stats = new Stats(); //the stats of the character
        List<int> traits = new List<int>(); 
        int spriteType = rng.Next(1, 5); //TODO: idk how many different sprite types there will be
        int parentBuilding = -1; //not yet defined
        bool hasJob = rng.Next(2) == 0;

        attributes.morality = rng.Next(0, 101);
        attributes.intelligence = rng.Next(0, 101);
        attributes.rationality = rng.Next(0, 101);
        attributes.fortitude = rng.Next(0, 101);
        attributes.charisma = rng.Next(0, 101);
        attributes.aesthetic_sensitivity = rng.Next(0, 101);
        attributes.perception = rng.Next(0, 101);
        attributes.dexterity = rng.Next(0, 101);
        attributes.constitution = rng.Next(0, 101);
        attributes.wisdom = rng.Next(0, 101);
        attributes.strength = rng.Next(0, 101);

        stats.condition = rng.Next(0, 101);
        stats.nutrition = rng.Next(0, 101);
        stats.happiness = rng.Next(0, 101);
        stats.energy = rng.Next(0, 101);
        stats.food = rng.Next(0, 101);
        stats.wealth = rng.Next(0, 101);

        int numberOfTraits = rng.Next(0, maxTrait);
        int traitNum;
        for (int j = 0; j < numberOfTraits; j++)
        {
            //gets a trait ID number that isn't already in the traits list
            do
            {
                traitNum = rng.Next(0, maxTrait);
            } while (traits.Contains(traitNum));

            traits.Add(traitNum); //adss the trait to the list
        }

        var inst = Instantiate(npcPrefab); //instantiates the NPC
        inst.transform.SetParent(dataController.npcContainer, false);
        var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

        npc.Load(id, npcName, attributes, stats, traits, spriteType, parentBuilding, hasJob); //loads the npc with data

        return npc;
    }

    public void generateSingleNPC()
    {
        System.Random rng = new System.Random();

        //gets the lowest missing key
        int id = -1;
        int lastKey = -1;
        foreach (int key in dataController.NPCStorage.Keys)
        {
            //checks if there are any gaps in the key id list
            if (key - 1 != lastKey)
            {
                id = key - 1;
                break;
            }
            lastKey = key;
        }

        //if no id has been set
        if (id == -1)
        {
            id = dataController.NPCStorage.Count; //the new id is the next key     
        }

        var npc = generateNPC(id, dataController.TraitStorage.Count, rng);
        dataController.NPCStorage.Add(npc.id, npc);
    }
} 
