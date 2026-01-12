using UnityEngine;

[System.Serliazable]
public class Relationship
{
    public RelationshipKey key;
    [Range(0,100)] public float value;

    public Relationship(){}

    public Relationship(RelationshipKey inputKey, float inputValue)
    {
        key = inputKey;
        value = inputValue;
    } 
}

/*
Wrapper for Relationship list that can be used as a stepping stone when serializing and de-serializing
*/
[System.Serializable]
public class RelationshipDatabase
{
    public List<Relationship> items = new List<Relationship>();
}
