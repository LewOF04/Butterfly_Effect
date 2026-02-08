using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionViewPrefab : MonoBehaviour
{
    public ActionInfoWrapper actionInfo;

    public TextMeshProUGUI actionNameField;
    public TextMeshProUGUI descriptionField; 

    public TextMeshProUGUI estUtilityField;
    public SpriteRenderer estUtilBg;

    public TextMeshProUGUI actUtilityField;
    public SpriteRenderer actUtilBg;

    public TextMeshProUGUI isKnownField;
    public SpriteRenderer isKnownBg;

    public TextMeshProUGUI estSuccessField;
    public SpriteRenderer estSuccBg;

    public TextMeshProUGUI actSuccessField;
    public SpriteRenderer actSuccBg;

    public TextMeshProUGUI timeField;
    public SpriteRenderer timeBg;

    public TextMeshProUGUI energyField;
    public SpriteRenderer energyBg;
    private DataController dataController = DataController.Instance;
    public void displayData()
    {
        actionNameField.text = "Action: "+actionInfo.action.name;
        descriptionField.text = "Description: "+actionInfo.action.baseDescription;
        estUtilityField.text = actionInfo.estUtility.ToString("0.00"); setColour(0f, 100f, actionInfo.estUtility, estUtilBg);
        actUtilityField.text = actionInfo.actUtility.ToString("0.00"); setColour(0f, 100f, actionInfo.actUtility, actUtilBg);

        if (actionInfo.known != true) { isKnownField.text = "No"; setColour(0f, 1f, 0f, isKnownBg);}
        else {isKnownField.text = "Yes"; setColour(0f, 1f, 1f, isKnownBg);}

        estSuccessField.text = actionInfo.estSuccess.ToString("0.00"); setColour(0f, 100f, actionInfo.estSuccess, estSuccBg);
        actSuccessField.text = actionInfo.actSuccess.ToString("0.00"); setColour(0f, 100f, actionInfo.actSuccess, actSuccBg);

        timeField.text = actionInfo.timeToComplete.ToString("0.00"); setColour(24f, 0f, actionInfo.timeToComplete, timeBg);
        energyField.text = actionInfo.energyToComplete.ToString("0.00"); setColour(100f, 0f, actionInfo.energyToComplete, energyBg);
    }

    private void setColour(float minVal, float maxVal, float val, SpriteRenderer background)
    {
        val = Mathf.Clamp(val, minVal, maxVal);
        float t = Mathf.InverseLerp(Mathf.Min(minVal, maxVal), Mathf.Max(minVal, maxVal), val);

        Color amber = new Color(1f, 0.75f, 0f);

        Color colour = t < 0.5f
            ? Color.Lerp(Color.red, amber, t * 2f)
            : Color.Lerp(amber, Color.green, (t - 0.5f) * 2f);

        background.color = colour;
    }

    public void actionClick()
    {
        if(actionInfo.known == false)
        {
            Debug.Log("Cannot perform action as it is unknown.");
        }
        else
        {
            
        }
    }
}
