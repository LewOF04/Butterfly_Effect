using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimelineUpdateItem : MonoBehaviour
{
    public IUpdateInfo updateInfo;
    public TextMeshProUGUI timeField;
    public void displayData(IUpdateInfo inputUpdateInfo) 
    {
        updateInfo = inputUpdateInfo;
        timeField.text = "Time: "+updateInfo.eventTime.ToString("0.00");

        if(updateInfo is AgentUpdateInfo agentInfo)
        {
            gameObject.GetComponent<Image>().color = new Color(0.8118f, 0.7725f, 0.0824f);

        } else if(updateInfo is AgentDeathInfo deathInfo)
        {
            gameObject.GetComponent<Image>().color = new Color(0.5020f, 0.0392f, 0.0157f);
        }
    }

    public void onClick()
    {
        OverviewMenu overviewMenu = FindFirstObjectByType<OverviewMenu>();

        ActionOverview actionOverview = overviewMenu.actionOverview;
        actionOverview.gameObject.SetActive(false);

        UpdateOverview updateOverview = overviewMenu.updateOverview;
        updateOverview.gameObject.SetActive(true);
        updateOverview.updateInfo = updateInfo;
        updateOverview.displayData();
    }
}