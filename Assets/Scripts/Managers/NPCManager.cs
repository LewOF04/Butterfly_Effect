using UnityEngine;
using System.Collections.Generic;
using System;

public class NPCManager : MonoBehaviour
{
    public NPC npcPrefab;

    [Header("Name Assets")]
    public TextAsset maleFirstNames;
    public TextAsset femaleFirstNames;
    public TextAsset neutralFirstNames;
    public TextAsset surnames;
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
        Attributes attributes = new Attributes(); //the attributes of the character
        Stats stats = new Stats(); //the stats of the character
        List<int> traits = new List<int>(); 
        int spriteType = rng.Next(1, 5);
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
        stats.energy = 100f;
        stats.food = rng.Next(0, 101);
        stats.wealth = rng.Next(0, 101);

        int numberOfTraits = rng.Next(0, maxTrait);
        int traitNum;
        
        for (int j = 0; j < numberOfTraits; j++)
        {
            //gets a trait ID number that isn't already in the traits list
            traitNum = rng.Next(0, maxTrait);
            
            while (traits.Contains(traitNum)) traitNum = rng.Next(0, maxTrait);

            traits.Add(traitNum); //adss the trait to the list
        }

        var inst = Instantiate(npcPrefab); //instantiates the NPC
        inst.transform.SetParent(dataController.npcContainer, false);
        var npc = inst.GetComponent<NPC>(); //gets the monoBehaviour script linked to the instantiation

        npc.Load(id, "", "", attributes, stats, traits, spriteType, parentBuilding, hasJob); //loads the npc with data
        generateAgentName(npc);
        npc.fullName = npc.firstName + " " + npc.surname;

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

    public Dictionary<int, NPCData> DeepClone()
    {
        Dictionary<int, NPC> npcStorage = dataController.NPCStorage;

        Dictionary<int, NPCData> outputDict = new Dictionary<int, NPCData>();

        List<int> npcKeys = new List<int>(npcStorage.Keys);
        foreach(int npcKey in npcKeys)
        {
            outputDict[npcKey] = new NPCData(npcStorage[npcKey]);
        }

        return outputDict;
    }

    //generates a first and last name for the agent
    private void generateAgentName(IAgent agent)
    {
        int seed = HashCode.Combine(
            agent.id,
            Mathf.RoundToInt(agent.stats.condition * 100f),
            Mathf.RoundToInt(agent.attributes.morality * 100f)
        );
        
        Debug.Log("Cond: "+agent.stats.condition.ToString() + " Morality: "+agent.attributes.morality.ToString());
        Debug.Log("NPC ID: "+agent.id.ToString() + " " + "Seed: "+seed.ToString());
        System.Random rng = new System.Random(seed); //weight based on agent info

        //randomly determine which name file we want to pull from
        TextAsset nameFile = (object) rng.Next(0, 3) switch
        {
            0 => maleFirstNames,
            1 => femaleFirstNames,
            _ => neutralFirstNames,
        };
        List<string> names = new List<string>(nameFile.text.Split('\n'));
        agent.firstName = GetRandomString(names, agent, rng);

        List<string> surnameList = new List<string>(surnames.text.Split("\n"));
        //check whether this agent has a building they live in
        if (DomainContext.DataController.TryGetBuilding(agent.parentBuilding, out IBuilding building))
        {
            if(building.familyName == "") //if there is no family name (no-one living there yet)
            {
                //get random surname
                agent.surname = GetRandomString(surnameList, agent, rng);
                building.familyName = agent.surname;
            }
            else
            {
                //if there is a family in this building, 80% chance they're part of that family
                int num = rng.Next(0, 101);
                if(num > 80) agent.surname = GetRandomString(surnameList, agent, rng);
                else agent.surname = building.familyName;
            }
        }
        else
        {
            //if no building, random surname
            agent.surname = GetRandomString(surnameList, agent, rng);   
        }
    }

    private string GetRandomString(List<string> strings, IAgent agent, System.Random rng)
    {
        int index = rng.Next(0, strings.Count);
        return strings[index].Trim();
    }
} 