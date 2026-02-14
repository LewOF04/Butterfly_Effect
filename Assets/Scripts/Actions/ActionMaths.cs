using UnityEngine;
using System.Collections.Generic;

public static class ActionMaths
{
    /*
    Given a value calculate the weighting for this value

    @param value - the value that we want to create a weighting from
    @param lowBound - the lower bound of the weighting number
    @param highBound - the higher bound of the weighting number
    @param boost - smaller the more this stat gets inflated

    @return - the weighting for this particular value
    */
    public static float calcMultiplier(float value, float minVal, float maxVal, float lowBound, float highBound)
    {
        float t = Mathf.InverseLerp(minVal, maxVal, value);
        return Mathf.Lerp(lowBound, highBound, t);
    }
    public static float scarcityMultiplier(float remainingAfter, float minValue, float maxValue, float lowRemainingMult, float highRemainingMult)
    {
        remainingAfter = Mathf.Clamp(remainingAfter, minValue, maxValue); //clamp between max and min values

        float temp = Mathf.InverseLerp(minValue, maxValue, remainingAfter); 

        return Mathf.Lerp(lowRemainingMult, highRemainingMult, temp);
    }

    public static float calcExpMultiplier(float value, float minVal, float maxVal, float lowBound, float highBound, float sharpness)
    {
        //sharpness of 2 gives the best result
        float t = Mathf.InverseLerp(minVal, maxVal, value); //calculate position between max and minimum possibilities
        float curvedT = Mathf.Pow(t, sharpness); //calculate exponential curve

        return Mathf.Lerp(lowBound, highBound, curvedT); //create multiplier from exponential and bounds
    }

    public static float rationalityNoise(float value, float rationality)
    {
        float r = Mathf.InverseLerp(0f, 100f, rationality);
        float swing = Mathf.Lerp(0.5f, 0f, r);       
        float multiplier = 1f + Random.Range(-swing, swing);
        return value * multiplier;
    }

    public static float ApplyWeightedMultipliers(float baseValue, List<float> multipliers, List<float> weights)
    {
        if (multipliers.Count != weights.Count) Debug.LogError("Multiplier and weight lists don't match length.");

        float weightedProduct = 1f; //counter for the total product
        float totalWeight = 0f; 

        for (int i = 0; i < multipliers.Count; i++) //for each multiplier
        {
            float multiplier = multipliers[i];
            float weight = weights[i];

            weightedProduct *= Mathf.Pow(multiplier, weight); //alters multiplier by weight
            totalWeight += weight; 
        }

        float combinedMultiplier = Mathf.Pow(weightedProduct, 1f / totalWeight);

        //if (totalWeight > 0f) weightedProduct = Mathf.Pow(weightedProduct, 1f / totalWeight); //alter by total weight to reduce huge weights
        return baseValue * combinedMultiplier;
    }

    public static float addChaos(float baseValue, float chaos)
    {
        if(chaos == 0) return baseValue;

        chaos = Mathf.InverseLerp(0f, 1f, chaos);

        float min = 1.0f - chaos;
        float max = 1.0f + chaos;

        float multiplier = Mathf.Lerp(min, max, Random.value);

        return baseValue * multiplier;
    }

    public static float addTraitWeights(NPC npc, float baseValue, List<int> traitList, bool positive)
    {
        foreach(int id in traitList)
        {
            if (npc.traits.Contains(id))
            {
                if(positive) baseValue += baseValue * 0.1f;
                else baseValue -= baseValue * 0.1f;
            }
        }
        return baseValue;
    }

    public static ActionResult calcActionSuccess(float successChance, float percentComplete)
    {
        float chance = Mathf.Clamp(successChance, 0f, 100f);
        float complete = Mathf.Clamp(percentComplete, 0f, 100f) / 100f;

        float roll = Random.Range(0f, 100f);

        float completionMult = Mathf.Lerp(0.5f, 1.5f, complete);
        float weightedChance = Mathf.Clamp(chance * completionMult, 0f, 100f);

        ActionResult result;
        result.success = roll <= weightedChance;

        float rollQuality = result.success
        ? Mathf.InverseLerp(weightedChance, 0f, roll)     //0 at barely success, 1 at great success
        : Mathf.InverseLerp(weightedChance, 100f, roll);  //0 at barely fail, 1 at awful fail

        float completionQuality = result.success
            ? complete          
            : (1f - complete);   

        result.quality = Mathf.Clamp01(rollQuality * 0.7f + completionQuality * 0.3f);

        return result;
    }
}

public struct ActionResult
{
    public bool success; //if the action succeeded or not
    public float quality; //how good/bad the actions success/failure was
}

