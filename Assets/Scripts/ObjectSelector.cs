using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ObjectSelector : MonoBehaviour
{
    //public GameObject BuildingMenu;
    private void OnMouseDown()
    {
        if (TryGetComponent<Building>(out var building))
        {
            Debug.Log("Clicked a Building!");
            int buildinID = building.id;
            string buildingName = building.buildingName;
            float buildingCond = building.condition;
            List<int> inhabitants = building.inhabitants;
        }
        else if (TryGetComponent<NPC>(out var npc))
        {
            Debug.Log("Clicked an NPC!");
            NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>();
            Canvas canvas = npcMenu.GetComponent<Canvas>();
            npcMenu.npc = npc;
            npcMenu.displayData();
            canvas.enabled = true;
        }
        else
        {
            Debug.Log("Clicked something else");
        }
    }
}