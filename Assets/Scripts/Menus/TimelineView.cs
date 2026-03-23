using UnityEngine;
using UnityEngine.UI;

public class TimelineView : MonoBehaviour
{
    public GameObject itemContainer;
    public TimelineItem itemPrefab;
    public TimelineUpdateItem updateItemPrefab;

    public void addItem(ISimEvent simEvent)
    {
        if(simEvent is SimulationActionWrapper simWrap)
        {
            TimelineItem newItem = Instantiate(itemPrefab, itemContainer.transform);
            newItem.displayData(simWrap);
        }
        if(simEvent is AgentUpdateInfo updateInfo)
        {
            TimelineUpdateItem newItem = Instantiate(updateItemPrefab, itemContainer.transform);
            newItem.displayData(updateInfo);
        }      
    }
}