using UnityEngine;
using System.Collections.Generic;
using System;

public class BuildingHistoryTracker : MonoBehaviour
{
    public Dictionary<BuildingRelationshipKey, int> largestInt; //stores the largest ints for each building history database
    public Dictionary<BuildingRelationshipKey, List<int>> missingInts; //stores the ints removed from the building history database

    public void Awake()
    {
        largestInt = new Dictionary<BuildingRelationshipKey, int>();
        missingInts = new Dictionary<BuildingRelationshipKey, List<int>>();
    }

    public void SetValues(List<int> inputLargestInt, List<BuildingRelationshipKey> largestIntKeys, List<List<int>> inputMissingInts, List<BuildingRelationshipKey> missingIntKeys)
    {
        largestInt = new Dictionary<BuildingRelationshipKey, int>();
        missingInts = new Dictionary<BuildingRelationshipKey, List<int>>();

        for(int i = 0; i < largestInt.Count; i++)
        {
            largestInt[largestIntKeys[i]] = inputLargestInt[i];
        }
        for(int j = 0; j < missingInts.Count; j++)
        {
            missingInts[missingIntKeys[j]] = inputMissingInts[j];
        }
    }

    /*
    Get the next integer which can represent a unique memory between a building and NPC
    We store this information so that upon deletion we can reuse keys so we don't run out
    */
    public int getNextInt(BuildingRelationshipKey rel)
    {
        //check if there aren't any missing ints we can set as the new integer instead
        if(missingInts[rel].Count == 0)
        {
            //no new int so take next largest int
            largestInt[rel]++;
            return largestInt[rel];
        }
        else
        {
            //there are missing ints so take the first available missing integer
            int nextInt = missingInts[rel][0];
            missingInts[rel].RemoveAt(0);

            return nextInt;
        }
    }

    /*
    Upon removing a memory we want to indicate that we can now reuse that integer at a later date as a new id
    */
    public void AddRemovedInt(BuildingRelationshipKey rel, int removedInt)
    {
        if(largestInt[rel] == removedInt) //if this is the largest int we just remove it entirely
        {
            largestInt[rel]--;
        }
        else
        { //if this isn't the largest int add it to the removedInts store
            missingInts[rel].Add(removedInt);
        }
    }
}

[System.Serializable]
public class BuildingHistoryTrackerDatabase
{
    public List<int> largestInts;
    public List<BuildingRelationshipKey> largestIntKeys;
    public List<List<int>> missingInts;
    public List<BuildingRelationshipKey> missingIntsKeys;

    public BuildingHistoryTrackerDatabase (Dictionary<BuildingRelationshipKey, int> inputLargestInts, Dictionary<BuildingRelationshipKey, List<int>> inputMissingInts){
        largestInts = new List<int>();
        largestIntKeys = new List<BuildingRelationshipKey>();
        missingInts = new List<List<int>>();
        missingIntsKeys = new List<BuildingRelationshipKey>();

        foreach(var kvp in inputLargestInts)
        {
            largestInts.Add(kvp.Value);
            largestIntKeys.Add(kvp.Key);
        }
        foreach(var kvp in inputMissingInts)
        {
            missingInts.Add(kvp.Value);
            missingIntsKeys.Add(kvp.Key);
        }
    }
}
