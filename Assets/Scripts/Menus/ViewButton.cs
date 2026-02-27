using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ViewButton : MonoBehaviour
{
    public Image spriteImageLoc;
    public TextMeshProUGUI title; 
    public MonoBehaviour source;
    public OverviewMenu overviewMenu;

    public void displayData()
    {
        if(source is NPC npc)
        {
            spriteImageLoc.sprite = npc.gameObject.GetComponent<SpriteRenderer>().sprite;
            spriteImageLoc.preserveAspect = true;
            title.text = "ID: "+npc.id;
        } else if (source is Building building)
        {
            spriteImageLoc.sprite = building.gameObject.GetComponent<SpriteRenderer>().sprite;
            spriteImageLoc.preserveAspect = true;
            title.text = "ID: "+building.id;  
        }
        
    }
    public void openSource()
    {
        //move the building canvas to the right place
        Camera camera = FindFirstObjectByType<Camera>();
        Canvas overviewCanvas = overviewMenu.GetComponent<Canvas>();
        
        if(source is NPC npc)
        {
            //get the npcMenu and buildingMenu canvas
            NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>();
            Canvas npcCanvas = npcMenu.GetComponent<Canvas>();
            npcMenu.transform.position = camera.transform.position;
            npcMenu.transform.position += new Vector3(0.0f, 0.0f, 5.0f);

            //update the data for the npc
            npcMenu.npc = npc;
            npcMenu.displayData();

            npcCanvas.enabled = true;
        } 
        
        else if (source is Building building)
        {
            //get the npcMenu and buildingMenu canvas
            BuildingMenu buildingMenu = FindFirstObjectByType<BuildingMenu>();
            Canvas buildingCanvas = buildingMenu.GetComponent<Canvas>();
            buildingMenu.transform.position = camera.transform.position;
            buildingMenu.transform.position += new Vector3(0.0f, 0.0f, 5.0f);

            //update the data for the npc
            buildingMenu.building = building;
            buildingMenu.displayData();

            buildingCanvas.enabled = true;
        }
        
        overviewCanvas.enabled = false;
        overviewMenu.transform.position -= new Vector3(0f, 0f, 5f);
        overviewMenu.removeButtons();
    }
}