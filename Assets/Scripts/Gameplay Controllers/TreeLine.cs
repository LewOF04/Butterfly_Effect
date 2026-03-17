using UnityEngine;

public class TreeLine : MonoBehaviour
{
    public Sprite[] treeLevels;
    public GameObject SideA;
    public GameObject SideB;

    public void setTrees(float avgCondition)
    {
        int level = Mathf.RoundToInt(Mathf.Lerp(0, treeLevels.Length - 1, avgCondition/100));

        foreach(Transform child in SideA.transform)
        {
            Sprite spriteToSet = getTreeType(level);
            child.gameObject.GetComponent<SpriteRenderer>().sprite = spriteToSet;
        }
        foreach(Transform child in SideB.transform)
        {
            Sprite spriteToSet = getTreeType(level);
            child.gameObject.GetComponent<SpriteRenderer>().sprite = spriteToSet;
        }
    }

    private Sprite getTreeType(int level)
    {
        level = Mathf.Clamp(level, 0, treeLevels.Length - 1);

        float totalWeight = 0f;
        float[] weights = new float[treeLevels.Length];

        for (int i = 0; i < treeLevels.Length; i++)
        {
            int distance = Mathf.Abs(i - level);

            float weight = 1f / (1f + distance * distance);

            weights[i] = weight;
            totalWeight += weight;
        }

        float roll = Random.Range(0f, totalWeight);

        for (int i = 0; i < treeLevels.Length; i++)
        {
            roll -= weights[i];
            if (roll <= 0f) return treeLevels[i];
        }

        return treeLevels[level];
    }
}
