using UnityEngine;
using System.Collections.Generic;

/*
 * These are the traits of the NPC, which help outline their personality and defines how they decide to take action.
 */
public class Attributes
{
    //all attributes range on a scale of 0-100 with 50 being considered the "average" level
    private float morality;
    private float intelligence;
    private float rationality;
    private float fortitude;
    private float charisma;
    private float aesthetic_sensitivity;
    private float perception;
    private float dexterity;
    private float constitution;
    private float wisdom;
    private float strength;

    //getter and setter functions for attributes
    public float Morality
    {
        get { return morality; }
        set { morality = value; }
    }
    public float Intelligence
    {
        get { return intelligence; }
        set { intelligence = value; }
    }
    public float Rationality
    {
        get { return rationality; }
        set { rationality = value; }
    }
    public float Fortitude
    {
        get { return fortitude; }
        set { fortitude = value; }
    }
    public float Charisma
    {
        get { return charisma; }
        set { charisma = value; }
    }
    public float Aesthetic_sensitivity
    {
        get { return aesthetic_sensitivity; }
        set { aesthetic_sensitivity = value; }
    }
    public float Perception
    {
        get { return perception; }
        set { perception = value; }
    }
}

/*
 * These are the stats of each NPC, which includes information about their current condition
 */
public class Stats
{
    //all statistics range from 0-100 with 50 being considered "standard" level
    private float condition;
    private float nutrition;
    private float happiness;
    private float energy;

    //quantity of food and money owned by the NPC
    private float food;
    private float wealth;

    //getter and setters for stats
    public float Condition
    {
        get { return condition; }
        set { condition = value; } 
    }
    public float Nutrition
    {
        get { return nutrition; }
        set { nutrition = value; }
    }
    public float Happiness
    {
        get { return happiness; }
        set { happiness = value; }
    }
    public float Energy
    {
        get{ return energy; }
        set { energy = value; }
    }
    public float Food
    {
        get{ return food; }
        set { food = value; }
    }
    public float Wealth
    {
        get{ return wealth; }
        set { wealth = value; }
    }

}

/*
 * The base class for each NPC
 */
public class NPC : MonoBehaviour
{
    private string id; //unique ID identifier for the NPC
    private string npcName; //name of the NPC
    private Attributes attributes; //the attributes of the character
    private Stats stats; //the stats of the character
    private List<int> traits; //list of ids of the traits that this NPC has
    private int buidlingID; //the building which the NPC belongs to (-1 if no such building exists)
    

    //getter and setters for NPC information
    public string Id
    {
        get { return id; }
        set { id = value; }
    }
    public string NpcName
    {
        get { return npcName; }
        set { npcName = value; }
    }
    public Attributes Attribute
    {
        get { return attributes; }
        set { attributes = value; } //TODO: not sure if this works as intended
    }
    public Stats Stat
    {
        get { return stats; }
        set { stats = value; }
    }
    public List<int> Traits
    {
        get { return traits; }
        set { traits = value; }
    }

    //extra utility functions for traits list
    public bool containsTrait(int id)
    {
        return traits.Contains(id);
    }
    public void addTrait(int id)
    {
        traits.Add(id);
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
