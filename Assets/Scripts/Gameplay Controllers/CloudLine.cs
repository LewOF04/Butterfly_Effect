using UnityEngine;

public class CloudLine : MonoBehaviour
{
    public GameObject SideA;
    public GameObject SideB;
    public bool isBack; //stores whether this is a back cloud (for transparency levels)

    public void setClouds(float avgCondition, float dayTime)
    {
        foreach(Transform child in SideA.transform)
        {
            SpriteRenderer spriteRend = child.gameObject.GetComponent<SpriteRenderer>();
            ApplyCloudColour(spriteRend, avgCondition, dayTime);
        }
        foreach(Transform child in SideB.transform)
        {
            SpriteRenderer spriteRend = child.gameObject.GetComponent<SpriteRenderer>();
            ApplyCloudColour(spriteRend, avgCondition, dayTime);
        }
    }

    private void ApplyCloudColour(SpriteRenderer spriteRend, float avgCondition, float dayTime)
    {
        Color conditionColor = GetConditionColour(avgCondition);
        Color timeColor = GetTimeColour(dayTime);

        spriteRend.color = conditionColor * timeColor; //combine the colours
    }

    private Color GetConditionColour(float avgCondition)
    {
        float val = avgCondition / 100f;

        float colorVariance = 0.15f;
        val = Mathf.Clamp01(val + Random.Range(-colorVariance, colorVariance));

        Color col = val > 0.4
        ? Color.Lerp(Color.gray, Color.white, Mathf.InverseLerp(0.4f, 1f, val))
        : Color.Lerp(Color.red, Color.gray, Mathf.InverseLerp(0f, 0.4f, val));

        //add chance of transparency based on the condition being higher
        if (val > 80 && Random.value < val) col.a = 0;
        else
        {
            if(isBack) col.a = Mathf.Clamp01(0.4549f + Random.Range(-0.3f, 0.3f)); 
            else col.a = Mathf.Clamp01(0.7451f + Random.Range(-0.3f, 0.3f));
        } 

        return col;
    }

    private Color GetTimeColour(float dayTime)
    {
        float darkness = GetDarkness(dayTime);

        Color dayColour = Color.white;
        Color nightColour = new Color(0.6f, 0.6f, 0.7f, 1f);

        return Color.Lerp(dayColour, nightColour, darkness);
    }

    private float GetDarkness(float dayTime)
    {
        if (dayTime >= 6f && dayTime <= 18f) return 0f; //daylight

        if (dayTime > 18f) return Mathf.InverseLerp(18f, 24f, dayTime); //sunset

        return Mathf.InverseLerp(6f, 0f, dayTime); //sunrise
    }
}
