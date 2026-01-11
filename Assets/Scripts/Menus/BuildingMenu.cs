using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
 
public class BuildingMenu : MonoBehaviour
{
    public Building building;
    public TextMeshProUGUI menuTitle;
    public Slider conditionSlider;
    public Image spriteImageLoc;
    public GameObject npcViewer;
    public NPCViewButton npcWindowPrefab;
    private DataController dataController = DataController.Instance;
    

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
        Debug.Log("History Clicked!");
    }
}
