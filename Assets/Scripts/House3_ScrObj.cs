using UnityEngine;

[CreateAssetMenu(fileName = "House3_ScrObj", menuName = "Scriptable Objects/House3_ScrObj")]
public class House3_ScrObj : ScriptableObject
{
    public static Vector3 startingPosition = new Vector3(1.21f, -0.1f, 0.0f);
    public float rentCost = 0;
    public float maintenanceCost = 0;
}
