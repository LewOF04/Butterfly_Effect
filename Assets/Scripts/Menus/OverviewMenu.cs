using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
 
public class OverviewMenu : MonoBehaviour
{
    [Header("Main Page")]
    public GameObject npcViewer;
    public GameObject buildingViewer;
    public ViewButton viewButtonPrefab; 
    public TextMeshProUGUI dateInfo;
    public TextMeshProUGUI timeInfo;
    public TextMeshProUGUI npcNumInfo;
    public TextMeshProUGUI buildingNumInfo;
    public TextMeshProUGUI avgCondInfo;

    [Header("Reload Confirmation")]
    public GameObject reloadConfirmationObj;

    [Header("Plot Simulation")]
    public GameObject plotSimulationConfirmationObj;
    public TextMeshProUGUI plotErrMsgField;
    public TMP_InputField plotTimeInput;

    [Header("Run Sim")]
    public GameObject runConfirmationObj; 
    public TextMeshProUGUI runErrMsgField;
    public Button runButton;

    [Header("View Plot")]
    public GameObject sidePanelButtons;
    public GameObject sidePanelInfo;
    public Button backButton;
    public Button viewButton;
    public GameObject buildingPanel;
    public GameObject agentPanel;
    public GameObject plotViewPanel;
    public GameObject agentsViewContainer;
    public GameObject timelineViewContainer;
    public TimelineAgentView timelineAgentPrefab;
    public TimelineView timelineViewPrefab;
    public ActionOverview actionOverview;

    private Dictionary<int, TimelineView> plotViewTimelines;

    [Header("Time Skipper")]
    private TimeSkipper skipperObject;
    private DataController dataController;

    public void Awake()
    {
        dataController = DataController.Instance;
    }

    public void displayData()
    {
        InputLocker.Lock(); //locks the input 
        reloadConfirmationObj.gameObject.SetActive(false);
        runConfirmationObj.gameObject.SetActive(false);
        skipperObject = FindFirstObjectByType<TimeSkipper>();
        backButton.gameObject.SetActive(false);
        sidePanelButtons.SetActive(true);
        sidePanelInfo.SetActive(true);
        plotViewPanel.SetActive(false);
        agentPanel.SetActive(true);
        buildingPanel.SetActive(true);
        actionOverview.gameObject.SetActive(false);

        if(skipperObject.simPlot == null)
        {
            runButton.interactable = false;
            viewButton.interactable = false;
        } 
        else 
        {
            runButton.interactable = true;
            viewButton.interactable = true;
        }

        //instantiate the buttons for the npcs
        Dictionary<int, NPC> npcs = dataController.NPCStorage;

        List<int> npcKeys = new List<int>(npcs.Keys);
        foreach (int id in npcKeys)
        {
            NPC npc = npcs[id];
            ViewButton newPrefab = Instantiate(viewButtonPrefab, npcViewer.transform);
            newPrefab.source = npc;
            newPrefab.overviewMenu = this;
            newPrefab.displayData();
        }

        //instantiate the buttons for the buildings
        Dictionary<int, Building> buildings = dataController.BuildingStorage;
        float totalCondition = 0;

        List<int> buildingKeys = new List<int>(buildings.Keys);
        foreach (int id in buildingKeys)
        {
            Building building = buildings[id];
            ViewButton newPrefab = Instantiate(viewButtonPrefab, buildingViewer.transform);
            newPrefab.source = building;
            newPrefab.overviewMenu = this;
            newPrefab.displayData();
            
            totalCondition += building.condition;
        }

        int totalBuildings = buildings.Count; buildingNumInfo.text = totalBuildings.ToString();
        int totalNPCs = npcs.Count; npcNumInfo.text = totalNPCs.ToString();
        float avgCondition = totalCondition / totalBuildings; avgCondInfo.text = avgCondition.ToString("0.00");

        WorldManager worldManager = dataController.worldManager;
        float timeInHours = worldManager.gameTime;

        //number of years
        float hoursPerYear = 24f * 365f;
        int years = Mathf.FloorToInt(timeInHours / hoursPerYear);
        timeInHours -= years * hoursPerYear;

        //number of hours
        float hoursPerMonth = 24f * 30.44f;
        int months = Mathf.FloorToInt(timeInHours / hoursPerMonth);
        timeInHours -= months * hoursPerMonth;

        //number of days
        float hoursPerDay = 24f;
        int days = Mathf.FloorToInt(timeInHours / hoursPerDay);
        timeInHours -= days * hoursPerDay;

        float minutesPerHour = 60f;
        int hours = Mathf.FloorToInt(timeInHours / 1);
        float minutes = (timeInHours - hours) * minutesPerHour;

        string daysString = days.ToString();
        string monthsString = months.ToString();
        string yearsString = years.ToString();
        if(daysString.Length < 2) daysString = "0"+daysString;
        if(monthsString.Length < 2) monthsString = "0"+monthsString;
        while(yearsString.Length < 4) yearsString = "0"+yearsString;
        dateInfo.text = daysString +"/"+monthsString+"/"+yearsString;

        string hoursString = hours.ToString();
        string minutesString = Mathf.RoundToInt(minutes).ToString();
        if(hoursString.Length < 2) hoursString = "0"+hoursString;
        if(minutesString.Length < 2) minutesString = "0"+minutesString;
        timeInfo.text = hoursString+":"+minutesString;

        gameObject.GetComponent<Canvas>().enabled = true;
    }

    /*
    Delete the buttons in the npc viewier
    */
    public void removeButtons()
    {
        foreach (Transform child in npcViewer.transform) Destroy(child.gameObject);
        foreach (Transform child in buildingViewer.transform) Destroy(child.gameObject);
        foreach(Transform child in agentsViewContainer.transform) Destroy(child.gameObject);
        foreach(Transform child in timelineViewContainer.transform) Destroy(child.gameObject);
    }

    public void exitMenu()
    {
        gameObject.transform.position -= new Vector3(0.0f, 0.0f, 5.0f);
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = false; //disable the canvas
        removeButtons();

        InputLocker.Unlock(); //unlock the inputs
    }

    public void reload()
    {
        reloadConfirmationObj.gameObject.SetActive(true);
    }
    public void reloadYes()
    {
        exitMenu();
        StartCoroutine(skipperObject.reload(true));
    }
    public void reloadNo()
    {
        reloadConfirmationObj.gameObject.SetActive(false);
    }

    public void plotSimulation()
    {
        plotSimulationConfirmationObj.gameObject.SetActive(true);
        plotErrMsgField.text = "";
    }

    public void plotYes()
    {
        if(float.TryParse(plotTimeInput.text, out float inputTime))
        {
            skipperObject.plotSim(inputTime);
            runButton.interactable = true;
            plotSimulationConfirmationObj.gameObject.SetActive(false);
            this.gameObject.transform.position -= new Vector3(0f,0f,5f);
        }
        else
        {
            plotErrMsgField.text = "Cannot parse input to float, please input a valid float or integer.";
        }
    }

    public void plotNo()
    {
        plotSimulationConfirmationObj.gameObject.SetActive(false);
    }

    public void viewPlot()
    {
        SimulationPlot simPlot = skipperObject.simPlot;
        sidePanelButtons.gameObject.SetActive(false);
        sidePanelInfo.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);

        plotViewPanel.SetActive(true);
        agentPanel.SetActive(false);
        buildingPanel.SetActive(false);

        //remove existing instantiated prefabs
        foreach(Transform child in agentsViewContainer.transform) Destroy(child.gameObject);
        foreach(Transform child in timelineViewContainer.transform) Destroy(child.gameObject);

        plotViewTimelines = new Dictionary<int, TimelineView>();

        List<SimulationActionWrapper> plottedActions = skipperObject.simPlot.extractPlottedActions(); //get a list of the actions that have been plotted
        
        //setup outline for all npcs
        foreach(NPC npc in dataController.NPCStorage.Values)
        {
            TimelineAgentView timelineAgentView = Instantiate(timelineAgentPrefab, agentsViewContainer.transform);
            timelineAgentView.displayData(npc);

            TimelineView timelineView = Instantiate(timelineViewPrefab, timelineViewContainer.transform);

            plotViewTimelines[npc.id] = timelineView;
        }

        //add all plotted actions to the layout
        foreach(SimulationActionWrapper simWrap in plottedActions)
        {
            ActionInfoWrapper info  = simWrap.info;
            int performerID = info.currentActor;

            TimelineView thisTimeline;
            try{thisTimeline = plotViewTimelines[performerID];} //select correct timeline, unless this performer is new
            catch
            {
                thisTimeline = Instantiate(timelineViewPrefab, timelineViewContainer.transform);
                plotViewTimelines[performerID] = thisTimeline;

                TimelineAgentView timelineAgentView = Instantiate(timelineAgentPrefab, agentsViewContainer.transform);
                timelineAgentView.displayData(performerID);
            }

            thisTimeline.addItem(simWrap);
        }
    }

    public void backToMain()
    {
        foreach(Transform child in agentsViewContainer.transform) Destroy(child.gameObject);
        foreach(Transform child in timelineViewContainer.transform) Destroy(child.gameObject);
        sidePanelButtons.gameObject.SetActive(true);
        sidePanelInfo.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);

        plotViewPanel.SetActive(false);
        agentPanel.SetActive(true);
        buildingPanel.SetActive(true);
        actionOverview.gameObject.SetActive(false);
    }

    public void runPlot()
    {
        runErrMsgField.text = "";
        runConfirmationObj.gameObject.SetActive(true);
    }

    public void runPlotYes()
    {   
        runErrMsgField.text = "";
        if(skipperObject.simPlot == null)
        {
            runErrMsgField.text = "There is no simulation plot to run, please create a simulation plot first.";
        }
        else
        {
            exitMenu();
            skipperObject.runFullSim();
            this.gameObject.transform.position -= new Vector3(0f,0f,5f);
        }
    }
         
    public void runPlotNo()
    {
        runConfirmationObj.gameObject.SetActive(false);
    }
}

