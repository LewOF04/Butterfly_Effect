using UnityEngine;
using System.Collections.Generic;

public class Trait : MonoBehaviour
{
    public int id; //unique ID number
    private string _displayName; //traits name
    private string _description; //short description of trait 

    private Dictionary<string, float> _numericalEffectss = new Dictionary<string, float>(); //dictionary of effects that alter attributes/statistics
    private List<string> _behaviouralEffects; //list of additional effects

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
