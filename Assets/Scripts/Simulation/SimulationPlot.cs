using System.Collections.Generic;

public class SimulationPlot
{
    public float startTime;
    public float endTime;
    public Dictionary<int, ActionInfoWrapper> activeAction;
    public DEQueue<ActionInfoWrapper> plottedActions;
    public Dictionary<int, int> agentInitiative;
    public DataControllerSnapshot domain;
    public SimulationPlot(DataControllerSnapshot dc)
    {
        domain = dc;
        List<int> agentKeys = new List<int>(dc.NPCStorage.Keys);
        
        foreach(int key in agentKeys)
        {
            activeActionQueues[key] = new DEQueue<ActionInfoWrapper>();
            completeActionQueues[key] = new DEQueue<ActionInfoWrapper>();
        }
    }
}