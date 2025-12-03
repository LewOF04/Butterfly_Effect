using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ObjectSelector : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (InputLocker.IsLocked == true)
        {
            return;
        }

        if (TryGetComponent<Building>(out var building))
        {
            Debug.Log("Clicked a Building!");
            BuildingMenu buildingMenu = FindFirstObjectByType<BuildingMenu>();
            Canvas canvas = buildingMenu.GetComponent<Canvas>();
            Camera camera = FindFirstObjectByType<Camera>();
            buildingMenu.transform.position = camera.transform.position;
            buildingMenu.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
            
            buildingMenu.building = building;
            buildingMenu.displayData();
            canvas.enabled = true;
        }
        
        else if (TryGetComponent<NPC>(out var npc))
        {
            Debug.Log("Clicked an NPC!");
            NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>(); //get the npc menu object
            Canvas canvas = npcMenu.GetComponent<Canvas>(); //get the canvas within the npc menu
            Camera camera = FindFirstObjectByType<Camera>();
            npcMenu.transform.position = camera.transform.position;
            npcMenu.transform.position += new Vector3(0.0f, 0.0f, 0.5f);

            npcMenu.npc = npc; //define the npc for the menu
            npcMenu.displayData(); //display the npcs data into the menu
            canvas.enabled = true; //make the canvas visible
        } 
        
        else if (TryGetComponent<TimeSkipper>(out var skipper))
        {
            skipper.reloadWorld();
        }
        
        else
        {
            Debug.Log("Clicked something else");
        }
    }
}