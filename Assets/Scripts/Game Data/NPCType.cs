using UnityEngine;

[CreateAssetMenu(
    fileName = "NewNPCType",
    menuName = "Scriptable Objects/NPC Type",
    order = 0)]
public class NPCType : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string race;
    public string hairColour;

    [Header("Visuals")]
    public Sprite[] possibleSprites = new Sprite[8];
    public Vector3 startingPosition;
    public Vector3 scale;
}