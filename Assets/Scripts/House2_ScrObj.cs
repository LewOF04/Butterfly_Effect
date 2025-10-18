using UnityEngine;

[CreateAssetMenu(fileName = "House2_ScrObj", menuName = "Scriptable Objects/House2_ScrObj")]
public class House2_ScrObj : ScriptableObject
{
    public static Vector3 startingPosition = new Vector3(1.21f, -0.63f, 0.0f);
    public float rentCost = 0;
    public float maintenanceCost = 0;
}
