using UnityEngine;
using System.Collections.Generic;
using System;

public class NPCManager : MonoBehaviour
{
    public NPC npcPrefab;

    const int MIN_NPCS = 0;
    const int MAX_NPCS = 10;
    //TODO: these numbers are entirely arbitrary

    //saving and loading

    public void SaveNPCs(Dictionary<string, NPC> NPCStorage)
    {
        SaveSys.SaveAllNPCs(NPCStorage);
    }

    public Dictionary<string, NPC> LoadNPCs()
    {
        var npcs = SaveSys.LoadAllNPCs(); //gets the list of stored NPCs

        Dictionary<string, NPC> NPCStorage = new Dictionary<string, NPC>(); //dictionary storing each NPC currently within the game 

        foreach (var npcData in npcs)
        {
            var inst = Instantiate(npcPrefab); //instantiates the NPC
            var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

            npc.Load(npcData); //loads the npc with data

            NPCStorage.Add(npc.id, npc); //adds the instantiated NPC to the storage
        }

        return NPCStorage;
    }

    public Dictionary<string, NPC> generateNPCs(System.Random rng, int maxTrait)
    {
        int npcNumber = rng.Next(MIN_NPCS, MAX_NPCS);

        Dictionary<string, NPC> NPCStorage = new Dictionary<string, NPC>(); //dictionary storing each NPC currently within the game 

        for (int i = 0; i < npcNumber; i++)
        {
            NPC npc = generateNPC(i.ToString(), maxTrait, rng);
            NPCStorage.Add(npc.id, npc);
        }

        return NPCStorage;
    }

    public NPC generateNPC(string inputID, int maxTrait, System.Random rng)
    {
        string id = inputID;
        string npcName = ""; //TODO: create a way to give NPCs unique names
        Attributes attributes = new Attributes(); //the attributes of the character
        Stats stats = new Stats(); //the stats of the character
        List<int> traits = new List<int>(); //TODO: idk how many traits there will be
        int spriteType = rng.Next(1, 5); //TODO: idk how many different sprite types there will be
        string parentBuilding = "-1"; //not yet defined

        attributes.morality = rng.Next(0, 100);
        attributes.intelligence = rng.Next(0, 100);
        attributes.rationality = rng.Next(0, 100);
        attributes.fortitude = rng.Next(0, 100);
        attributes.charisma = rng.Next(0, 100);
        attributes.aesthetic_sensitivity = rng.Next(0, 100);
        attributes.perception = rng.Next(0, 100);
        attributes.dexterity = rng.Next(0, 100);
        attributes.constitution = rng.Next(0, 100);
        attributes.wisdom = rng.Next(0, 100);
        attributes.strength = rng.Next(0, 100);

        stats.condition = rng.Next(0, 100);
        stats.nutrition = rng.Next(0, 100);
        stats.happiness = rng.Next(0, 100);
        stats.energy = rng.Next(0, 100);
        stats.food = rng.Next();
        stats.wealth = rng.Next();

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
        var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

        npc.Load(id, npcName, attributes, stats, traits, spriteType, parentBuilding); //loads the npc with data

        return npc;
    }
} 
