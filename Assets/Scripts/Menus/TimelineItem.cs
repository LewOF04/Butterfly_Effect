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
    }

    public void onClick()
    {
        
    }
}