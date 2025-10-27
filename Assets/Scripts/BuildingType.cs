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
    [Min(0)] public int purchaseCost;    // in your chosen currency units
    [Min(0)] public int maintenanceCost;
    [Min(0)] public int buildCost;

    [Header("Visuals")]
    public Sprite[] possibleSprites = new Sprite[8];  // drop all variants here (night/day, rotated, skin variants, etc.)
    public Vector3 scale;

    [Tooltip("Optional: default scale to apply to this building.")]
    public Vector3 defaultPosition = Vector3.one;
}