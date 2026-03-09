using System.Collections.Generic;
using System.Linq;

public class SimulationPlot
{
    //overall simulation info
    public float startTime;
    public float endTime;
    
    //simulation running info
    public bool runComplete = false;
    public float runTime;
    public DEQueue<SimulationActionWrapper> plottedActions; //final plotted actions

    //agent action information
    public Dictionary<int, SimulationActionWrapper> agentActiveActions;
    public List<int> inactiveAgents;

    //building action information
    public Dictionary<int, List<SimulationActionWrapper>> buildingActiveActions; //actions being performed to each building

    public DataControllerSnapshot domain;
    public SimulationPlot(DataControllerSnapshot dc)
    {
        domain = dc;
        plottedActions = new DEQueue<SimulationActionWrapper>();
        inactiveAgents = new List<int>();
        
        List<int> agentKeys = new List<int>(dc.NPCStorage.Keys);
        foreach(int key in agentKeys) agentActiveActions[key] = null;

        List<int> buildingKeys = new List<int>(dc.BuildingStorage.Keys);
        foreach(int key in buildingKeys) buildingActiveActions[key] = new List<SimulationActionWrapper>();
    }
}