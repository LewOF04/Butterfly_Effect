using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NPCMenu : MonoBehaviour
{
    [Header("General Info")]
    public NPC npc;
    public TextMeshProUGUI menuTitle;
    public Button backToMainButton;
    public Button saveButton;
    
    [Header("Main Page")]
    public List<Slider> attributeSliders = new List<Slider>();
    public List<Slider> statSliders = new List<Slider>();
    public Button buildingButton;
    public Image spriteImageLoc;
    public GameObject traitViewer;
    public TraitViewItem traitItemPrefab;
    public Canvas mainCanvas; 

    [Header("Relationship Page")]
    public Canvas relationshipCanvas;
    public GameObject relationshipInfoContainer;
    public RelationshipPanel relationshipPrefab;

    [Header("Memory Page")]
    public Canvas memoryCanvas;
    public GameObject memoryInfoContainer;
    public NPCMemoryPanel npcMemoryPrefab;
    public BuildingMemoryPanel buildingMemoryPrefab;

    [Header("Action Page")]
    public Canvas actionCanvas;
    public GameObject npcView;
    public GameObject npcContainer;
    public GameObject ActionView;
    public GameObject actionContainer;
    public NPCViewPrefab<NPC> actionNPCView;
    public NPCViewPrefab<Building> actionBuildingView;
    public NPCActionPrefab<NPC> npcActPrefab;
    public NPCActionPrefab<Building> buildingActPrefab;
    public NonNPCActionPrefab envOrSelfPrefab;


    private DataController dataController = DataController.Instance;
    

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

        foreach(Transform child in relationshipInfoContainer.transform)
        {
            RelationshipPanel info = child.gameObject.GetComponent<RelationshipPanel>();
            info.save(); 
        }
    }

    public void displayData()
    {
        InputLocker.Lock(); //locks the input 

        //main page
        Attributes attrs = npc.attributes;
        Stats stats = npc.stats;

        if(npc.parentBuilding < 0)
        {
            buildingButton.enabled = false;
        }
        else
        {
            buildingButton.enabled = true;
        }

        spriteImageLoc.sprite = npc.GetComponent<SpriteRenderer>().sprite;
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

        List<int> npcTraits = npc.traits;
        Dictionary<int, TraitData> traitDatabase = dataController.TraitStorage;
        foreach (int id in npcTraits)
        {
            TraitData trait = traitDatabase[id];
            TraitViewItem newPrefab = Instantiate(traitItemPrefab, traitViewer.transform);
            newPrefab.trait = trait;
            newPrefab.displayData();
        }

        //relationship page
        foreach(Relationship relationship in dataController.RelationshipPerNPCStorage[npc.id])
        {
            RelationshipPanel relInsts = Instantiate(relationshipPrefab, relationshipInfoContainer.transform);
            relInsts.relationship = relationship;
            if(relationship.key.npcA == npc.id) relInsts.secondaryNPC = dataController.NPCStorage[relationship.key.npcB];
            else relInsts.secondaryNPC = dataController.NPCStorage[relationship.key.npcA];

            relInsts.displayData();
        }

        //memory page
        List<NPCEvent> thisNPCEvents = dataController.eventsPerNPCStorage[npc.id];
        foreach(NPCEvent thisEvent in thisNPCEvents)
        {
            NPCMemoryPanel memPanel = Instantiate(npcMemoryPrefab, memoryInfoContainer.transform); 
            
            memPanel.performer = npc;
            memPanel.thisEvent = thisEvent;

            memPanel.displayData();
        }

        List<BuildingEvent> thisBuildingEvents = dataController.buildingEventsPerNPCStorage[npc.id];
        foreach(BuildingEvent thisEvent in thisBuildingEvents)
        {
            BuildingMemoryPanel memPanel = Instantiate(buildingMemoryPrefab, memoryInfoContainer.transform);

            memPanel.building = dataController.BuildingStorage[thisEvent.eventKey.building];
            memPanel.thisEvent = thisEvent;

            memPanel.displayData();
        }

        mainCanvas.enabled = true;
        backToMainButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(true);
    }

    public void exitMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas
        removeButtons();

        backToMainButton.gameObject.SetActive(false);
        relationshipCanvas.enabled = false;
        InputLocker.Unlock(); //unlock the inputs

    }

    public void backToMain()
    {
        backToMainButton.gameObject.SetActive(false);
        if(relationshipCanvas.enabled == true) relationshipCanvas.enabled = false;
        if(memoryCanvas.enabled == true)
        {
            memoryCanvas.enabled = false;
            saveButton.gameObject.SetActive(true);
        }

        mainCanvas.enabled = true;
    }

    public void openActions()
    {
        Debug.Log("Clicked Actions!");
    }

    public void openMemories()
    {
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        memoryCanvas.enabled = true;
        saveButton.gameObject.SetActive(false);
    }

    public void openRelationships()
    {
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        relationshipCanvas.enabled = true;
    }

    public void openBuilding()
    {
        if (npc.parentBuilding >= 0)
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.enabled = false; //disable the canvas
            var buildings = dataController.BuildingStorage;
            Building building = buildings[npc.parentBuilding];
            BuildingMenu buildingMenu = FindFirstObjectByType<BuildingMenu>();
            Canvas buidlingCanvas = buildingMenu.GetComponent<Canvas>();
            Camera camera = FindFirstObjectByType<Camera>();
            buildingMenu.transform.position = camera.transform.position;
            buildingMenu.transform.position += new Vector3(0.0f, 0.0f, 5.0f);

            gameObject.transform.position -= new Vector3(0.0f, 0.0f, 5.0f);
            
            buildingMenu.building = building;
            buildingMenu.displayData();

            removeButtons();
            buidlingCanvas.enabled = true;
        }
    }

    /*
    Delete the any instantiated prefabs within the menu
    */
    public void removeButtons()
    {
        foreach (Transform child in traitViewer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in relationshipInfoContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in memoryInfoContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

