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

    }

    public void exitMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas

        InputLocker.Unlock(); //unlock the inputs

    }

    public void viewHistory()
    {
        Debug.Log("History Clicked!");
    }
}
