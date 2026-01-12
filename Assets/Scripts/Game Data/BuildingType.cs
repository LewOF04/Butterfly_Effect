using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBuildingType",
    menuName = "Scriptable Objects/Building Type",
    order = 0)]
public class BuildingType : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Economy")]
    [Min(0)] public int purchaseCost;  
    [Min(0)] public int maintenanceCost;
    [Min(0)] public int buildCost;
    [Min(1)] public int capacity;

    [Header("Visuals")]
    public Sprite[] possibleSprites = new Sprite[8]; 
    public Vector3 scale;
    public Vector3 defaultPosition = Vector3.one;
    public Vector2[] colliderShape;
}