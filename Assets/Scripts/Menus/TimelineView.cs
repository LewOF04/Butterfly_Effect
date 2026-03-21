using UnityEngine;
using UnityEngine.UI;

public class TimelineView : MonoBehaviour
{
    public GameObject itemContainer;
    public TimelineItem itemPrefab;

    public void addItem(SimulationActionWrapper simWrap)
    {
        TimelineItem newItem = Instantiate(itemPrefab, itemContainer.transform);
        newItem.displayData(simWrap);
    }
}