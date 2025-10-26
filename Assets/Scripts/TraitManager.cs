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

        Debug.Log($"Loaded Traits from: {traitFile}");
        
        Dictionary<int, TraitData> traitStorage = new Dictionary<int, TraitData>(); //dictionary storing each trait possible 

        foreach (var traitData in traits)
        {
            traitStorage.Add(traitData.id, traitData); //adds the trait to the storage
        }

        return traitStorage;
    }
}
