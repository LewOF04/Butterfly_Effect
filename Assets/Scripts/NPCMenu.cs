using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NPCMenu : MonoBehaviour
{
    public NPC npc;
    public TextMeshProUGUI menuTitle;
    public List<Slider> attributeSliders = new List<Slider>();
    public List<Slider> statSliders = new List<Slider>();
    public Button buildingButton;
    public Image spriteImageLoc;
    

    public void saveNPC()
    {
        Attributes attrs = npc.attributes;
        Stats stats = npc.stats;

        attrs.morality = attributeSliders[0].value;
        attrs.intelligence = attributeSliders[1].value;
        attrs.rationality = attributeSliders[2].value;
        attrs.fortitude = attributeSliders[3].value;
        attrs.charisma = attributeSliders[4].value;
        attrs.aesthetic_sensitivity = attributeSliders[5].value;
        attrs.perception = attributeSliders[6].value;
        attrs.dexterity = attributeSliders[7].value;
        attrs.constitution = attributeSliders[8].value;
        attrs.wisdom = attributeSliders[9].value;
        attrs.strength = attributeSliders[10].value;

        stats.condition = statSliders[0].value;
        stats.nutrition = statSliders[1].value;
        stats.happiness = statSliders[2].value;
        stats.energy = statSliders[3].value;
        stats.food = statSliders[4].value;
        stats.wealth = statSliders[5].value;
    }

    public void displayData()
    {
        InputLocker.Lock(); //locks the input 

        Attributes attrs = npc.attributes;
        Stats stats = npc.stats;

        if(npc.parentBuilding == -1)
        {
            buildingButton.enabled = false;
        }
        else
        {
            buildingButton.enabled = true;
        }

        Sprite npcSprite = npc.GetComponent<SpriteRenderer>().sprite;
        spriteImageLoc.sprite = npcSprite;
        spriteImageLoc.preserveAspect = true;

        menuTitle.text = "(NPC) Name: "+npc.npcName+" ID: "+npc.id;

        attributeSliders[0].value = attrs.morality;
        attributeSliders[1].value = attrs.intelligence;
        attributeSliders[2].value = attrs.rationality;
        attributeSliders[3].value = attrs.fortitude;
        attributeSliders[4].value = attrs.charisma;
        attributeSliders[5].value = attrs.aesthetic_sensitivity;
        attributeSliders[6].value = attrs.perception;
        attributeSliders[7].value = attrs.dexterity;
        attributeSliders[8].value = attrs.constitution;
        attributeSliders[9].value = attrs.wisdom;
        attributeSliders[10].value = attrs.strength;

        statSliders[0].value = stats.condition;
        statSliders[1].value = stats.nutrition;
        statSliders[2].value = stats.happiness;
        statSliders[3].value = stats.energy;
        statSliders[4].value = stats.food;
        statSliders[5].value = stats.wealth;
    }

    public void exitMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas

        InputLocker.Unlock(); //unlock the inputs

    }
}

