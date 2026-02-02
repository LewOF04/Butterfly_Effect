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
    @param influence - how much the stat itself influences the outputted weight

    @return - the weighting for this particular value
    */
    public static float calcWeighting(float value, float lowBound, float highBound, float influence)
    {
        float normal = Mathf.Clamp01(value);
        normal = Mathf.Clamp01(normal);
        normal = Mathf.Pow(normal, statWeighting);
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
}

