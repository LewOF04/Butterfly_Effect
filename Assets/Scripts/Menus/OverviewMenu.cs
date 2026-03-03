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

    [Header("Skip Time Confirmation")]
    public GameObject skipConfirmationObj; 
    public TextMeshProUGUI skipErrMsgField;
    public TMP_InputField timeInput;

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
        skipConfirmationObj.gameObject.SetActive(false);
        skipperObject = FindFirstObjectByType<TimeSkipper>();

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

        string daysString = years.ToString();
        string monthsString = months.ToString();
        string yearsString = years.ToString();
        if(daysString.Length < 2) daysString = "0"+daysString;
        if(monthsString.Length < 2) monthsString = "0"+monthsString;
        while(yearsString.Length < 4) yearsString = "0"+yearsString;
        dateInfo.text = daysString +"/"+monthsString+"/"+yearsString;

        string hoursString = hours.ToString();
        string minutesString = minutes.ToString();
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
        foreach (Transform child in npcViewer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in buildingViewer.transform) 
        {
            Destroy(child.gameObject);
        }
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
        skipperObject.reloadWorld();
        exitMenu();
    }
    public void reloadNo()
    {
        reloadConfirmationObj.gameObject.SetActive(false);
    }

    public void skipTime()
    {
        skipErrMsgField.text = "";
        skipConfirmationObj.gameObject.SetActive(true);
    }
    public void skipTimeYes()
    {   
        skipErrMsgField.text = "";
        if(float.TryParse(timeInput.text, out float inputTime))
        {
            skipperObject.skipTime(inputTime);
            exitMenu();
        }
        else
        {
            skipErrMsgField.text = "Cannot parse input to float, please input a valid float or integer.";
        }
    }
         
    public void skipTimeCancel()
    {
        skipConfirmationObj.gameObject.SetActive(false);
    }
}

