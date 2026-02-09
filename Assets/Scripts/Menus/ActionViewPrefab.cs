using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NoTarget = System.ValueTuple;

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
        NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>();
        npcMenu.errorTextField.text = "";
        Debug.Log("Action clicked");
        if(actionInfo.known == false) {npcMenu.errorTextField.text = "Cannot perform action as it is unknown."; return;}
        if(actionInfo.timeToComplete > npcMenu.npc.timeLeft) {npcMenu.errorTextField.text = "Cannot perform action as the NPC does not have enough time."; return;}
        if(actionInfo.energyToComplete > npcMenu.npc.stats.energy) {npcMenu.errorTextField.text = "Cannot perform action as the NPC does not have enough energy."; return;}
        
        npcMenu.actionConfirm.gameObject.SetActive(true);
        npcMenu.actionConfirm.actionNameField.text = "Action: "+actionInfo.action.name;
        npcMenu.actionConfirm.sourceAction = this;
        
        Debug.Log("Action performed");
    }

    public void actionConfirmed()
    {
        IActionBase act = actionInfo.action;

        var relKey = new RelationshipKey(actionInfo.currentActor, actionInfo.receiver);
        if(act is ActionBase<NPC> npcAct)
        {
            npcAct.reloadAction(actionInfo);
            npcAct.performAction(100f);
        } else if(act is ActionBase<Building> buildAction)
        {
            buildAction.reloadAction(actionInfo);
            buildAction.performAction(100f); 
        } else if(act is ActionBase<NoTarget> nonAction)
        {
            nonAction.reloadAction(actionInfo);
            nonAction.performAction(100f); 
        }
    }
}
