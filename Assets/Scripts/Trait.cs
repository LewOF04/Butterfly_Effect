using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Trait")]
public class TraitDefinition : ScriptableObject
{
    public string id;              // unique key, e.g. "Strong"
    public string displayName;
    public string description;
    public float damageMultiplier; 
    // add fields as needed
}

public class Trait : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
