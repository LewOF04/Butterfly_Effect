using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NonNPCActionPrefab
{
    ActionInfoWrapper actionInfo;

    public TextMeshProUGUI actionNameField;
    public TextMeshProUGUI descriptionField;

    public TextMeshProUGUI estUtilityField;
    public Image estUtilBg;

    public TextMeshProUGUI actUtilityField;
    public Image actUtilBg;

    public TextMeshProUGUI isKnownField;
    public Image isKnownBg;

    public TextMeshProUGUI estSuccessField;
    public Image estSuccBg;

    public TextMeshProUGUI actSuccessField;
    public Image actSuccBg;

    public TextMeshProUGUI timeField;
    public Image timeBg;

    public TextMeshProUGUI energyField;
    public Image energyBg;
    private DataController dataController = DataController.Instance;
    public void displayData()
    {
        actionNameField.text = actionInfo.action.name;
        descriptionField.text = actionInfo.action.baseDescription;
        estUtilityField.text = actionInfo.estUtility.ToString("0.000"); setColour(0f, 100f, actionInfo.estUtility, estUtilBg);
        actUtilityField.text = actionInfo.actUtility.ToString("0.000"); setColour(0f, 100f, actionInfo.actUtility, actUtilBg);
        
        if(actionInfo.known != true) {isKnownField.text = "Unknown"; setColour(0f, 1f, 1f, isKnownBg);}
        else {isKnownField.text = "Known"; setColour(0f, 1f, 0f, isKnownBg);}

        estSuccessField.text = actionInfo.estSuccess.ToString("0.00") + "%"; setColour(0f, 100f, actionInfo.estSuccess, estSuccBg);
        actSuccessField.text = actionInfo.actSuccess.ToString("0.00") + "%"; setColour(0f, 100f, actionInfo.actSuccess, actSuccBg);

        timeField.text = actionInfo.timeToComplete.ToString("0.00") + "hrs"; setColour(24f, 0f, actionInfo.timeToComplete, timeBg);
        energyField.text = actionInfo.energyToComplete.ToString("0.00"); setColour(50f, 0f, actionInfo.energyToComplete, energyBg);
    }

    private void setColour(float minVal, float maxVal, float val, Image background)
    {
        val = Mathf.Clamp(val, minVal, maxVal);
        float t = Mathf.InverseLerp(minVal, maxVal, val);

        Color colour = t < 0.5f
            ? Color.Lerp(Color.green, new Color(1f, 0.75f, 0f), t * 2f)
            : Color.Lerp(new Color(1f, 0.75f, 0f), Color.red, (t - 0.5f) * 2f);

        background.color = colour;
    }
}