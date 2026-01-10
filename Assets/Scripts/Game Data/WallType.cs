using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWallType",
    menuName = "Scriptable Objects/Wall Type",
    order = 0)]
public class WallType : ScriptableObject
{
    [Header("Visuals")]
    public Sprite[] possibleSprites = new Sprite[8];
    public Vector3 scale;
    public GameObject leftPrefab;
    public GameObject rightPrefab;
    public Vector3 defaultTransform;
}