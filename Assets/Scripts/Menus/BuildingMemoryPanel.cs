using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class BuildingMemoryPanel : MonoBehaviour
{
    public Building building;
    public NPC npc;
    public BuildingEvent thisEvent;
    public TextMeshProUGUI actionNameField;
    public TextMeshProUGUI descriptionField;
    public TextMeshProUGUI buildingIDField;
    public TextMeshProUGUI npcIDField;
    public TextMeshProUGUI buildingNameField;
    public TextMeshProUGUI npcNameField;
    public Image buildingSpriteImageLoc;
    public Image npcSpriteImageLoc;
    public TextMeshProUGUI buildImpField;
    public TextMeshProUGUI npcImpField;
    public DataController dataController = DataController.Instance;

    public void displayData()
    {
        actionNameField.text = "Event: "+thisEvent.actionName;
        descriptionField.text = thisEvent.description;
        buildingIDField.text = "Building ID: "+building.id.ToString();
        buildingNameField.text = "Building Name: "+building.buildingName;
        buildImpField.text = "Importance: "+thisEvent.buildingImportance;
        buildingSpriteImageLoc.sprite = building.GetComponent<SpriteRenderer>().sprite;

        if(thisEvent.eventKey.npc == -1)
        {
            npcNameField.text = "No Target";
            npcIDField.text = "";
            npcSpriteImageLoc.sprite = null; 
            npcImpField.text = ""; 
        }
        else
        {
            npc = dataController.NPCStorage[thisEvent.eventKey.npc];
            npcNameField.text = "Name: "+npc.npcName;
            npcIDField.text = "ID: "+npc.id.ToString();
            npcSpriteImageLoc.sprite = npc.GetComponent<SpriteRenderer>().sprite;
            npcImpField.text = "Importance: " + thisEvent.npcImportance;
        }
    }
}
