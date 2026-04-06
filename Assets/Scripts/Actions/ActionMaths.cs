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

        float frac = Mathf.InverseLerp(minValue, maxValue, remainingAfter); 

        float curvedT = Mathf.Pow(frac, 5f);

        return Mathf.Lerp(lowRemainingMult, highRemainingMult, curvedT);
    }

    public static float calcExpMultiplier(float value, float minVal, float maxVal, float lowBound, float highBound, float sharpness)
    {
        float temp = Mathf.InverseLerp(minVal, maxVal, value);

        //apply exponential curve
        float curvedTemp = Mathf.Pow(temp, sharpness);

        //if decreasing range, invert the curve
        if (lowBound > highBound) curvedTemp = 1f - curvedTemp;

        return Mathf.Lerp(lowBound, highBound, curvedTemp);
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

    public static float addTraitWeights(IAgent npc, float baseValue, List<int> traitList, bool positive)
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
        float chance = Mathf.Clamp(successChance, 0f, 100f) / 100f; //translate success chance to fraction
        float complete = Mathf.Clamp(percentComplete, 0f, 100f) / 100f; //translate success chance to fraction

        //create random generator
        int seed = (successChance.GetHashCode() + percentComplete.GetHashCode()).GetHashCode();
        System.Random rng = new System.Random(seed);

        float completionFactor = Mathf.Lerp(0.5f, 1f, complete); //define how much we want the proportion of completion to factor the chance

        float weightedChance = chance * completionFactor; //weight the success chance contributor by the completion factor
        float roll = (float) rng.NextDouble(); //roll a random value

        ActionResult result;
        result.success = roll <= weightedChance; //determine roll success

        float rollQuality = result.success //the quality of the success is a factor of the overall chance and the roll
            ? Mathf.InverseLerp(weightedChance, 0f, roll)
            : Mathf.InverseLerp(weightedChance, 1f, roll);

        float completionQuality = result.success
            ? complete
            : (1f - complete);

        float baseQuality = rollQuality * 0.3f + completionQuality * 0.7f;
        float contrast = 1.5f; 
        result.quality = Mathf.Clamp01(Mathf.Pow(baseQuality, contrast));

        return result;
    }
}

public struct ActionResult
{
    public bool success; //if the action succeeded or not
    public float quality; //how good/bad the actions success/failure was
}

