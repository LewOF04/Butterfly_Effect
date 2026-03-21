using System.Collections.Generic;
public static class SimulationView
{
    public static List<int> getPlotAgents(List<SimulationActionWrapper> actions)
    {
        List<int> agents = new List<int>();

        foreach(SimulationActionWrapper act in actions)
        {
            int actor = act.info.currentActor;
            if(!agents.Contains(actor)) agents.Add(actor);
        }
        return agents;
    }
}