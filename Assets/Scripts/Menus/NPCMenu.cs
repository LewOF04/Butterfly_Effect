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
    public NPCViewPrefab npcViewPref;
    public ActionViewPrefab actionViewPref;
    public TextMeshProUGUI errorTextField;
    public ActionConfirm actionConfirm;

    [Header("<i>Buttons</i>")]
    public Button npcActButton;
    public Button buildActButton;
    public Button selfActButton;
    public Button envActButton;
    private ActionFrontier possibleActions;
    [Header("<i>Receiver Info</i>")]
    public GameObject recGameObj;
    public Image recSpriteLoc;
    public TextMeshProUGUI recNameField;
    public TextMeshProUGUI recIDField;

    private DataController dataController = DataController.Instance;
    

    /*
    When save button is clicked, take all changes made to the values and set them to the npc
    */
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

        //reset action calculations
        possibleActions = new ActionFrontier(dataController);
        possibleActions.produceFrontier(npc);
    }

    /*
    Load the NPC menu with the data of the npc
    */
    public void displayData()
    {
        InputLocker.Lock(); //locks the input 

        //main page
        Attributes attrs = npc.attributes;
        Stats stats = npc.stats;

        //determine whether to show parent building
        if(npc.parentBuilding < 0)
        {
            buildingButton.enabled = false;
        }
        else
        {
            buildingButton.enabled = true;
        }

        //set npc values into main page
        spriteImageLoc.sprite = npc.GetComponent<SpriteRenderer>().sprite;
        spriteImageLoc.preserveAspect = true;

        menuTitle.text = "(NPC) Name: "+npc.npcName+" ID: "+npc.id.ToString();

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

        //populate traits data storage
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

        recGameObj.gameObject.SetActive(false);
        ActionView.gameObject.SetActive(false);
        npcView.gameObject.SetActive(false);

        mainCanvas.enabled = true;
        backToMainButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(true);
        actionConfirm.gameObject.SetActive(false);

        //calculate all actions available to npc
        possibleActions = new ActionFrontier(dataController);
        possibleActions.produceFrontier(npc);
    }

    /*
    When the exit button is clicked, reset the menu and close
    */
    public void exitMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas
        removeButtons(); //remove prefabs

        backToMainButton.gameObject.SetActive(false);

        relationshipCanvas.enabled = false;
        actionCanvas.enabled = false;
        memoryCanvas.enabled = false;

        gameObject.transform.position -= new Vector3(0.0f, 0.0f, 5.0f);

        InputLocker.Unlock(); //unlock the inputs
    }

    /*
    When the back button on the sub pages is clicked, return the main menu page
    Reset any containers and gameobjects
    */
    public void backToMain()
    {
        backToMainButton.gameObject.SetActive(false);

        //if returning from the relationship page
        if(relationshipCanvas.enabled == true) relationshipCanvas.enabled = false;

        //if returning from the memory page
        if(memoryCanvas.enabled == true)
        {
            memoryCanvas.enabled = false;
            saveButton.gameObject.SetActive(true);
        }

        //if returning from the action page
        if(actionCanvas.enabled == true)
        {
            actionCanvas.enabled = false;
            saveButton.gameObject.SetActive(true);
            recGameObj.gameObject.SetActive(false);
            removeNPCViews();
            removeActionViews();
        }

        mainCanvas.enabled = true;
    }

    /*
    Open the actions sub page of the npc menu
    */
    public void openActions()
    {
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        actionCanvas.enabled = true;

        envActButton.interactable = true;
        selfActButton.interactable = true;
        buildActButton.interactable = true;
        npcActButton.interactable = true;

        errorTextField.text = "";
        saveButton.gameObject.SetActive(false);
    }

    /*
    Open the memory sub page of the npc menu
    */
    public void openMemories()
    {
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        memoryCanvas.enabled = true;
        saveButton.gameObject.SetActive(false);
    }

    /*
    Open the relationship sub page of the npc menu
    */
    public void openRelationships()
    {
        mainCanvas.enabled = false;
        backToMainButton.gameObject.SetActive(true);
        relationshipCanvas.enabled = true;
    }

    /*
    Open the building menu from the reference in the npc menu
    */
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
            
            buildingMenu.building = building;
            buildingMenu.displayData();
            gameObject.transform.position -= new Vector3(0.0f, 0.0f, 5.0f);

            removeButtons();
            buidlingCanvas.enabled = true;
        }
    }

    /*
    Upon clicking for self actions, show all actions that can be performed to themselves.
    */
    public void selfActPress()
    {
        ActionView.gameObject.SetActive(true); //show the container for the actions

        //set the buttons to the relevant setup
        selfActButton.interactable = false;
        buildActButton.interactable = true;
        envActButton.interactable = true;
        npcActButton.interactable = true;
        recGameObj.gameObject.SetActive(false);

        //remove any existing prefabs in the containers
        removeActionViews();
        removeNPCViews();

        //iterate over all self actions and instantiate the relevant prefab
        foreach(ActionInfoWrapper info in possibleActions.selfActions)
        {
            ActionViewPrefab inst = Instantiate(actionViewPref, actionContainer.transform);
            inst.actionInfo = info;
            inst.displayData();
        }

        errorTextField.text = "";
        //ensure the npc view is hidden
        npcView.gameObject.SetActive(false);
    }

    /*
    Upon clicking for environment actions, show all actions that we could perform to the environment
    */
    public void envActPres()
    {
        ActionView.gameObject.SetActive(true);

        envActButton.interactable = false;
        selfActButton.interactable = true;
        buildActButton.interactable = true;
        npcActButton.interactable = true;
        recGameObj.gameObject.SetActive(false);

        removeActionViews();
        removeNPCViews();
        foreach(ActionInfoWrapper info in possibleActions.environmentActions)
        {
            ActionViewPrefab inst = Instantiate(actionViewPref, actionContainer.transform);
            inst.actionInfo = info;
            inst.displayData();
        }

        errorTextField.text = "";
        npcView.gameObject.SetActive(false);
    }

    /*
    Upon clicking for npc actions, show each npc that we could perform an action to
    */
    public void npcActButtPress()
    {
        npcView.gameObject.SetActive(true);

        npcActButton.interactable = false;
        selfActButton.interactable = true;
        buildActButton.interactable = true;
        envActButton.interactable = true;
        recGameObj.gameObject.SetActive(false);

        removeActionViews();
        removeNPCViews();

        //iterate over all npcs and create the prefab to view the npcs information
        Dictionary<int, NPC> npcs = dataController.NPCStorage;
        List<int> npcKeys = new List<int>(npcs.Keys);
        foreach(int npcKey in npcKeys)
        {
            if(npcKey == npc.id) continue;
            NPC thisNPC = npcs[npcKey];
            NPCViewPrefab inst = Instantiate(npcViewPref, npcContainer.transform);
            inst.performer = npc;
            inst.receiver = thisNPC;
            inst.displayData();
        }

        errorTextField.text = "";
        ActionView.gameObject.SetActive(false);
    }

    /*
    Upon clicking for building actions, show each building that we could perform an action to
    */
    public void buildActButtPress()
    {
        npcView.gameObject.SetActive(true);

        buildActButton.interactable = false;
        selfActButton.interactable = true;
        envActButton.interactable = true;
        npcActButton.interactable = true;
        recGameObj.gameObject.SetActive(false);

        removeActionViews();
        removeNPCViews();

        Dictionary<int, Building> buildings = dataController.BuildingStorage;
        List<int> buildingKeys = new List<int>(buildings.Keys);
        foreach(int buildingKey in buildingKeys)
        {
            Building building = buildings[buildingKey];
            NPCViewPrefab inst = Instantiate(npcViewPref, npcContainer.transform);
            inst.performer = npc;
            inst.receiver = building;
            inst.displayData();
        }

        errorTextField.text = "";
        ActionView.gameObject.SetActive(false);
    }

    /*
    Show Npc and Building Actions upon click of NPC or Building view prefab
    */
    public void showNPCBuildActs(MonoBehaviour receiver)
    {
        //check that the relevant objects are set correctly
        recGameObj.gameObject.SetActive(true);
        ActionView.gameObject.SetActive(true);
        removeActionViews();

        //get the actions which could be performed to this building/npc
        List<ActionInfoWrapper> actions = new List<ActionInfoWrapper>();
        if(receiver is Building building) {
            actions = possibleActions.buildingActions[building.id];
        }
        if(receiver is NPC thisNpc) {
            actions = possibleActions.npcActions[thisNpc.id];
        }

        //iterate over all actions and display them
        foreach(ActionInfoWrapper info in actions)
        {
            ActionViewPrefab inst = Instantiate(actionViewPref, actionContainer.transform);
            inst.actionInfo = info;
            inst.displayData();
        }

        removeNPCViews();
        npcView.gameObject.SetActive(false);
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
        removeNPCViews();
        removeActionViews();
    }

    public void removeNPCViews()
    {
        foreach(Transform child in npcContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void removeActionViews()
    {
        foreach(Transform child in actionContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

