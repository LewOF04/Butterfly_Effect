using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewNPCType",
    menuName = "Scriptable Objects/NPC Type",
    order = 0)]
public class NPCType : ScriptableObject
{
    [Header("Identity")]
    public string id;

    [Header("Visuals")]
    //sprite level, for each level 4 sprites [0] = standing sprite, [1,2,3] walking sprites
    public Sprite[] level0Sprites;
    public Sprite[] level1Sprites;
    public Sprite[] level2Sprites;
    public Sprite[] level3Sprites;
    public Sprite[] level4Sprites;
    public Sprite[] level5Sprites;
    public Sprite[] level6Sprites;
    public Sprite[] level7Sprites;
    public Vector3 startingPosition;
    public Vector3 scale;

    public List<Sprite[]> getSprites()
    {
        return new List<Sprite[]>{level0Sprites, level1Sprites, level2Sprites, level3Sprites, level4Sprites, level5Sprites, level6Sprites, level7Sprites};
    }
}