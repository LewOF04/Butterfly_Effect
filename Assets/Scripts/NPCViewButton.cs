using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NPCViewButton : MonoBehaviour
{
    public Image spriteImageLoc;
    public TextMeshProUGUI npcTitle; 
    public NPC npc;

    public void displayData()
    {
        spriteImageLoc.sprite = npc.gameObject.GetComponent<SpriteRenderer>().sprite;
        spriteImageLoc.preserveAspect = true;
        npcTitle.text = "ID: "+npc.id;
    }
    public void openNPC()
    {
        //get the npcMeny and buildingMenu canvas
        NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>();
        Canvas npcCanvas = npcMenu.GetComponent<Canvas>();
        BuildingMenu buildingMenu = FindFirstObjectByType<BuildingMenu>();
        Canvas buildingCanvas = buildingMenu.GetComponent<Canvas>();

        //move the building canvas to the right place
        Camera camera = FindFirstObjectByType<Camera>();
        npcMenu.transform.position = camera.transform.position;
        npcMenu.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
        
        //update the data for the npc
        npcMenu.npc = npc;
        npcMenu.displayData();

        //make the npc canvas viewable and the building canvas invisible
        buildingCanvas.enabled = false;
        npcCanvas.enabled = true;

        //delete the gameObjects of the npc buttons
        buildingMenu.removeButtons();
    }
}