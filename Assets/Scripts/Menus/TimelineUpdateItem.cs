using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimelineUpdateItem : MonoBehaviour
{
    public AgentUpdateInfo updateInfo;
    public TextMeshProUGUI timeField;
    public void displayData(AgentUpdateInfo inputUpdateInfo)
    {
        updateInfo = inputUpdateInfo;
        timeField.text = "Time: "+updateInfo.eventTime.ToString("0.00");
        gameObject.GetComponent<Image>().color = new Color(0.549f, 0.012f, 0.988f);
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