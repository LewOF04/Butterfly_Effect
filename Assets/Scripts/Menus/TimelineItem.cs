using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimelineItem : MonoBehaviour
{
    public SimulationActionWrapper simWrap;
    public TextMeshProUGUI startTimeField;
    public TextMeshProUGUI endTimeField;
    public TextMeshProUGUI receiverField;
    public TextMeshProUGUI actionNameField;
    public RectTransform rectTransform;
    public void displayData(SimulationActionWrapper inputSimWrap)
    {
        simWrap = inputSimWrap;
        ActionInfoWrapper actionWrap = simWrap.info;

        startTimeField.text = "Start time: "+simWrap.startTime.ToString("0.00");
        endTimeField.text = "End time: "+simWrap.endTime.ToString("0.00");
        
        if(actionWrap.receiver == -1) receiverField.text = "No Receiver";
        else receiverField.text = receiverField.text = "Receiver ID: "+actionWrap.receiver.ToString();

        actionNameField.text = actionWrap.action.name;

        float duration = simWrap.endTime - simWrap.startTime;
        if(duration < 1) duration = 1f;

        rectTransform.sizeDelta = new Vector2 (240, 150*duration);

        Image img = gameObject.GetComponent<Image>();
        if(actionWrap.action is BuildingAction buildAct)
        {
            img.color = new Color(0.4941f, 0.0784f, 0.6f);
        }else if(actionWrap.action is NPCAction npcAct)
        {
            img.color = new Color(0.0667f, 0.4353f, 0.9490f);
        }else if(actionWrap.action is EnvironmentAction envAct)
        {
            img.color = new Color(0.0353f, 0.7020f, 0.1686f);
        }else if(actionWrap.action is SelfAction selfAct)
        {
            img.color = new Color(0.7686f, 0.4118f, 0.1137f);
        }
    } 

    public void onClick()
    {
        OverviewMenu overviewMenu = FindFirstObjectByType<OverviewMenu>();

        UpdateOverview updateOverview = overviewMenu.updateOverview;
        updateOverview.gameObject.SetActive(false);

        ActionOverview actionOverview = overviewMenu.actionOverview;
        actionOverview.gameObject.SetActive(true);
        actionOverview.actionInfo = simWrap.info;
        actionOverview.displayData();
    }
}