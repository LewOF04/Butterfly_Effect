using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionOverview : MonoBehaviour
{
    public ActionInfoWrapper actionInfo;

    public TextMeshProUGUI actionNameField;
    public TextMeshProUGUI descriptionField; 

    public TextMeshProUGUI estUtilityField;
    public Image estUtilBg;

    public TextMeshProUGUI actUtilityField;
    public Image actUtilBg;

    public TextMeshProUGUI estSuccessField;
    public Image estSuccBg;

    public TextMeshProUGUI actSuccessField;
    public Image actSuccBg;

    public TextMeshProUGUI timeField;
    public Image timeBg;

    public TextMeshProUGUI energyField;
    public Image energyBg;
    
    public TextMeshProUGUI receiverIDField;
    private DataController dataController = DataController.Instance;
    public void displayData()
    {
        actionNameField.text = "Action: "+actionInfo.action.name;
        descriptionField.text = "Description: "+actionInfo.action.baseDescription;
        estUtilityField.text = actionInfo.estUtility.ToString("0.00"); setColour(0f, 100f, actionInfo.estUtility, estUtilBg);
        actUtilityField.text = actionInfo.actUtility.ToString("0.00"); setColour(0f, 100f, actionInfo.actUtility, actUtilBg);

        estSuccessField.text = actionInfo.estSuccess.ToString("0.00"); setColour(0f, 100f, actionInfo.estSuccess, estSuccBg);
        actSuccessField.text = actionInfo.actSuccess.ToString("0.00"); setColour(0f, 100f, actionInfo.actSuccess, actSuccBg);

        timeField.text = actionInfo.timeToComplete.ToString("0.00"); setColour(24f, 0f, actionInfo.timeToComplete, timeBg);
        energyField.text = actionInfo.energyToComplete.ToString("0.00"); setColour(100f, 0f, actionInfo.energyToComplete, energyBg);

        if(actionInfo.receiver == -1) receiverIDField.text = "No Receiver";
        else receiverIDField.text = "Receiver ID: "+actionInfo.receiver.ToString();
    }

    private void setColour(float minVal, float maxVal, float val, Image background)
    {
        val = Mathf.Clamp(val, minVal, maxVal);
        float t = Mathf.InverseLerp(Mathf.Min(minVal, maxVal), Mathf.Max(minVal, maxVal), val);

        Color amber = new Color(1f, 0.75f, 0f);

        Color colour = t < 0.5f
            ? Color.Lerp(Color.red, amber, t * 2f)
            : Color.Lerp(amber, Color.green, (t - 0.5f) * 2f);

        background.color = colour;
    }
}
