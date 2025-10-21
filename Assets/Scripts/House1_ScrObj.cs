using UnityEngine;

[CreateAssetMenu(fileName = "House1_ScrObj", menuName = "Scriptable Objects/House1_ScrObj")]
public class House1_ScrObj : ScriptableObject
{
    public static Vector3 startingPosition = new Vector3(1.21f, -0.96f, 0.0f); //defines house starting position based on asset size
    public float rentCost = 0;
    public float maintenanceCost = 0;
}
