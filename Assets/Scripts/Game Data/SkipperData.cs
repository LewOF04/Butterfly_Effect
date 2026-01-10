using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkipperType",
    menuName = "Scriptable Objects/Skipper Type",
    order = 0)]
public class SkipperData : ScriptableObject
{
    public Sprite[] possibleSprites = new Sprite[8];
    public Vector3 scale;
    public Vector3 defaultTransform;
    public Vector2[] colliderShape;
}