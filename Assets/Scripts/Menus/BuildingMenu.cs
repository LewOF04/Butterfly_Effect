using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
 
public class BuildingMenu : MonoBehaviour
{
    public Building building;
    public TextMeshProUGUI menuTitle;

    [Header("Main Page")]
    public Canvas mainCanvas;
    public Slider conditionSlider;
    public Image spriteImageLoc;
    public GameObject npcViewer;
    public NPCViewButton npcWindowPrefab;

    [Header("History Page")]
    public Canvas historyCanvas;
    public GameObject memoryInfoContainer;
    public BuildingMemoryPanel buildingMemoryPrefab;


    [Header("Others")]
    private DataController dataController = DataController.Instance;
    public Button backToMainButton;
    public Button saveButton;
    

    public void saveBuilding()
    {
        building.condition = conditionSlider.value;
    }

    public void displayData()
    {
        InputLocker.Lock(); //locks the input 

        Sprite buildingSprite = building.GetComponent<SpriteRenderer>().sprite;
        spriteImageLoc.sprite = buildingSprite;
        spriteImageLoc.preserveAspect = true;

        menuTitle.text = "(Building) Name: "+building.buildingName+" ID: "+building.id;

        conditionSlider.value = building.condition;

        //instantiate the buttons for the npcs of the Building
        List<int> npcs = building.inhabitants;
        foreach (int id in npcs)
        {
            NPC npc = dataController.NPCStorage[id];
            NPCViewButton newPrefab = Instantiate(npcWindowPrefab, npcViewer.transform);
            newPrefab.npc = npc;
            newPrefab.displayData();
        }

        List<BuildingEvent> thisBuildingEvents = dataController.buildingEventsPerBuildingStorage[building.id];
        foreach(BuildingEvent thisEvent in thisBuildingEvents)
        {
            BuildingMemoryPanel memPanel = Instantiate(buildingMemoryPrefab, memoryInfoContainer.transform);

            memPanel.building = building;
            memPanel.thisEvent = thisEvent;

            memPanel.displayData();
        }

        historyCanvas.enabled = false;
        mainCanvas.enabled = true;
        backToMainButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(true);
    }

    /*
    Return to the main canvas after being in a menu subpage
    */
    public void backToMain()
    {
        historyCanvas.enabled = false;
        mainCanvas.enabled = true;
        backToMainButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(true);
    }

    /*
    Delete the buttons in the npc viewier
    */
    public void removeButtons()
    {
        foreach (Transform child in npcViewer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in memoryInfoContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void exitMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas
        removeButtons();

        InputLocker.Unlock(); //unlock the inputs
    }

    public void viewHistory()
    {
        historyCanvas.enabled = true;
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(false);
    }
}
