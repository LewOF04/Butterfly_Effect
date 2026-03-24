using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UIElements;
using TMPro;

public class ObjectSelector : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (InputLocker.IsLocked == true) return;

        if (TryGetComponent<Building>(out Building building)) openBuildingMenu(building);
        
        else if (TryGetComponent<NPC>(out NPC npc)) openNPCMenu(npc);
        
        else if (TryGetComponent<TimeSkipper>(out TimeSkipper _)) openOverviewMenu();

        else if (TryGetComponent<SpeechBubble>(out SpeechBubble bubble)) openSpeech(bubble);
    }
    private void openOverviewMenu()
    {
        OverviewMenu overMenu = FindFirstObjectByType<OverviewMenu>();
        Canvas canvas = overMenu.GetComponent<Canvas>();
        Camera camera = FindFirstObjectByType<Camera>();
        overMenu.transform.position = camera.transform.position;
        overMenu.transform.position += new Vector3(0f, 0f, 5f);

        overMenu.displayData();
        canvas.enabled = true; 
    }

    private void openNPCMenu(NPC npc)
    {
        NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>(); //get the npc menu object
        Canvas canvas = npcMenu.GetComponent<Canvas>(); //get the canvas within the npc menu
        Camera camera = FindFirstObjectByType<Camera>();
        npcMenu.transform.position = camera.transform.position;
        npcMenu.transform.position += new Vector3(0f, 0f, 5f);

        npcMenu.npc = npc; //define the npc for the menu
        npcMenu.displayData(); //display the npcs data into the menu
        canvas.enabled = true; //make the canvas visible
    }

    private void openBuildingMenu(Building building)
    {
        BuildingMenu buildingMenu = FindFirstObjectByType<BuildingMenu>();
        Canvas canvas = buildingMenu.GetComponent<Canvas>();
        Camera camera = FindFirstObjectByType<Camera>();
        buildingMenu.transform.position = camera.transform.position;
        buildingMenu.transform.position += new Vector3(0f, 0f, 5f);
        
        buildingMenu.building = building;
        buildingMenu.displayData();
        canvas.enabled = true;
    }

    private void openSpeech(SpeechBubble speechBubble)
    {
        AgentSpeechWindow speechWindow = FindFirstObjectByType<AgentSpeechWindow>();
        speechWindow.gameObject.GetComponent<Canvas>().enabled = true;

        string speech = speechBubble.getSpeech();
        speechBubble.gameObject.SetActive(false);

        speechWindow.setSpeech(speech);
        speechWindow.agentNameField.text = "Agent: "+speechBubble.parentNPC.fullName;
        InputLocker.Lock();
    }
}