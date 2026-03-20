using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        agentActiveActions = new Dictionary<int, SimulationActionWrapper>();
        foreach(int key in agentKeys) agentActiveActions[key] = null;

        List<int> buildingKeys = new List<int>(dc.BuildingStorage.Keys);
        buildingActiveActions = new Dictionary<int, List<SimulationActionWrapper>>();
        foreach(int key in buildingKeys) buildingActiveActions[key] = new List<SimulationActionWrapper>();
    }

    public List<SimulationActionWrapper> extractPlottedActions()
    {
        if(plottedActions == null) return null;

        List<SimulationActionWrapper> actions = new List<SimulationActionWrapper>();
        List<SimulationActionWrapper> temp = new List<SimulationActionWrapper>();

        //read through plotted actions list
        while (plottedActions.TryDequeueFront(out SimulationActionWrapper simAct))
        {
            actions.Add(simAct);
            temp.Add(simAct);
        }

        //save back to plottedActions
        foreach (SimulationActionWrapper act in temp)
        {
            plottedActions.EnqueueBack(act);
        }

        return actions;
    }
}