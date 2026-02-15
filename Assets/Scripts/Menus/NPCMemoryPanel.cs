using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCMemoryPanel : MonoBehaviour
{
    public NPC performer;
    public NPC receiver;
    public NPCEvent thisEvent;
    public TextMeshProUGUI actionNameField;
    public TextMeshProUGUI descriptionField;
    public TextMeshProUGUI performerIDField;
    public TextMeshProUGUI receiverIDField;
    public TextMeshProUGUI performerNameField;
    public TextMeshProUGUI receiverNameField;
    public Image performerSpriteImageLoc;
    public Image receiverSpriteImageLoc;
    public TextMeshProUGUI perfImpField;
    public TextMeshProUGUI recImpField;
    public TextMeshProUGUI receiverText;
    public TextMeshProUGUI wasPosText;

    [Header("Was Positive")]
    public SpriteRenderer wasPosPerfField;
    public SpriteRenderer wasPosRecField;
    private DataController dataController;
    public void Awake()
    {
        dataController = DataController.Instance;
    }

    public void displayData()
    {
        actionNameField.text = "Event: "+thisEvent.actionName;
        descriptionField.text = thisEvent.description;
        performerIDField.text = "ID: "+performer.id.ToString();
        performerNameField.text = "Name: "+performer.npcName;
        perfImpField.text = "Importance: "+thisEvent.performerImportance;
        performerSpriteImageLoc.sprite = performer.GetComponent<SpriteRenderer>().sprite;
        if(thisEvent.wasPositivePerf) wasPosPerfField.color = Color.green;
        else wasPosPerfField.color = Color.red;

        if(thisEvent.receiver == -1)
        {
            wasPosText.gameObject.SetActive(false);
            //hide the receiver section of the memory
            receiverNameField.text = "";
            receiverIDField.text = "";
            receiverSpriteImageLoc.gameObject.SetActive(false);
            recImpField.text = "";
            receiverText.text = "No Target";
            wasPosRecField.gameObject.SetActive(false);
        }
        else
        {
            //make sure all receiver game objects are active
            receiverSpriteImageLoc.gameObject.SetActive(true);
            wasPosRecField.gameObject.SetActive(true);
            wasPosText.gameObject.SetActive(true);

            //populate receiver field
            Debug.Log("Receiver Num = "+thisEvent.receiver.ToString());
            
            receiver = dataController.NPCStorage[thisEvent.receiver];
            receiverNameField.text = "Name: "+receiver.npcName;
            receiverIDField.text = "ID: "+receiver.id.ToString();
            receiverSpriteImageLoc.sprite = receiver.GetComponent<SpriteRenderer>().sprite;
            recImpField.text = "Importance: "+thisEvent.receiverImportance;
            receiverText.text = "Receiver";
            if(thisEvent.wasPositiveRec) wasPosRecField.color = Color.green;
            else wasPosRecField.color = Color.red;
        }
    }
}
