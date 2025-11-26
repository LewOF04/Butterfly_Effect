using UnityEngine;
using UnityEngine.EventSystems;

public class EventClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private void Awake()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Complete Click");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}