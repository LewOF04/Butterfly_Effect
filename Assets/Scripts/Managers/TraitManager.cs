using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TraitManager : MonoBehaviour
{
    public TextAsset traitFile; 

    //loading traits
    public Dictionary<int, TraitData> LoadTraits()
    {
        string json = traitFile.text; //reads the traitDatabase from the json
        var data = JsonUtility.FromJson<TraitDatabase>(json); //returns to a TraitDatabase from the json format
        var traits = data.items;
        
        Dictionary<int, TraitData> traitStorage = new Dictionary<int, TraitData>(); //dictionary storing each trait possible 

        foreach (var trait in traits)
        {
            traitStorage.Add(trait.id, trait); //adds the trait to the storage
        }

        return traitStorage;
    }
}
