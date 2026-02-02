using System.Random;
using UnityEngine;

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

    public static float rationalityNoise(float value)
    {
        float range = Mathf.Clamp01(value);
        float min = 1.0f - range;
        float max = 1.0f + range;

        float t = (Random.value + Random.value) * 0.5f;
        return Mathf.Lerp(min, max, t);
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

    public static float addChaosRandomness(float baseValue, float chaos)
    {
        if(chaos = 0) return baseValue;

        chaos = Mathf.Clamp01(chaos);

        float min = 1.0f - chaos;
        float max = 1.0f + chaos;

        float t = (Random.value + Random.value) * 0.5f;

        float multiplier = Mathf.Lerp(min, max, t);

        return baseValue * multiplier;
    }
}

