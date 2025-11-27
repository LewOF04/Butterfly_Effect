using UnityEngine;
using UnityEngine.EventSystems;

public class EventClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("CLICKED");
    }
}