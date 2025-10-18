using UnityEngine;

[CreateAssetMenu(fileName = "House0_ScrObj", menuName = "Scriptable Objects/House0_ScrObj")]
public class House0_ScrObj : ScriptableObject
{
    public static Vector3 startingPosition = new Vector3(1.21f, -0.926f, 0.0f);
    public float rentCost = 0;
    public float maintenanceCost = 0;
    
}
