using UnityEngine;

[CreateAssetMenu(fileName = "House4_ScrObj", menuName = "Scriptable Objects/House4_ScrObj")]
public class House4_ScrObj : ScriptableObject
{
    public static Vector3 startingPosition = new Vector3(1.21f, 0.03f, 0.0f);
    public float rentCost = 0;
    public float maintenanceCost = 0;
}
