using UnityEngine;
using System.Collections.Generic;

public static class ActionMaths
{
    public static float normalize(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }

    /*
    Given a value calculate the weighting for this value

    @param value - the value that we want to create a weighting from
    @param lowBound - the lower bound of the weighting number
    @param highBound - the higher bound of the weighting number
    @param boost - smaller the more this stat gets inflated

    @return - the weighting for this particular value
    */
    public static float calcMultiplier(float value, float lowBound, float highBound)
    {
        float normal = Mathf.Clamp01(value);
        //normal = Mathf.Pow(normal, boost);
        return Mathf.Lerp(lowBound, highBound, normal);
    }

    public static float rationalityNoise(float value, float rationality)
    {
        float range = Mathf.Clamp01(rationality);
        float min = 1.0f - range;
        float max = 1.0f + range;

        float multiplier = Mathf.Lerp(min, max, Random.value);

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

        //if (totalWeight > 0f) weightedProduct = Mathf.Pow(weightedProduct, 1f / totalWeight); //alter by total weight to reduce huge weights
        return baseValue * weightedProduct;
    }

    public static float addChaos(float baseValue, float chaos)
    {
        if(chaos == 0) return baseValue;

        chaos = Mathf.Clamp01(chaos);

        float min = 1.0f - chaos;
        float max = 1.0f + chaos;

        float multiplier = Mathf.Lerp(min, max, Random.value);

        return baseValue * multiplier;
    }

    public static float scarcityMultiplier(float remainingAfter, float minValue, float maxValue, float lowRemainingMult, float highRemainingMult)
    {
        remainingAfter = Mathf.Clamp(remainingAfter, minValue, maxValue); //clamp between max and min values

        float temp = Mathf.InverseLerp(minValue, maxValue, remainingAfter); 

        return Mathf.Lerp(lowRemainingMult, highRemainingMult, temp);
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

    public static ActionResult calcActionSuccess(float successChance)
    {
        float roll = Random.Range(0f, 100f);

        ActionResult result;
        result.success = roll <= successChance;

        if (result.success) result.quality = Mathf.InverseLerp(successChance, 0f, roll); //closer to the roll the better success
        else result.quality = Mathf.InverseLerp(successChance, 100f, roll); //further from the roll the worse the failure

        return result;
    }
}

public struct ActionResult
{
    public bool success; //if the action succeeded or not
    public float quality; //how good/bad the actions success/failure was
}

