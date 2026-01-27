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
    public DataController dataController = DataController.Instance;

    public void displayData()
    {
        actionNameField.text = thisEvent.actionName;
        descriptionField.text = thisEvent.description;
        performerIDField.text = "ID: "+performer.id.ToString();
        performerNameField.text = "Name: "+performer.npcName;

        if(thisEvent.receiver == -1)
        {
            receiverNameField.text = "No Target";
            receiverIDField.text = "";
            receiverSpriteImageLoc.sprite = null; 
        }
        else
        {
            receiver = dataController.NPCStorage[thisEvent.receiver];
            receiverNameField.text = "Name: "+receiver.npcName;
            receiverIDField.text = "ID: "+receiver.id.ToString();
            receiverSpriteImageLoc.sprite = performer.GetComponent<SpriteRenderer>().sprite;
        }
    }
}
