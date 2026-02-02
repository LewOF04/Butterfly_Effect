using UnityEngine;
using System.Collections.Generic;

public class ActionFrontier
{
    public Action bestAction;
    public Dictionary<int, List<ActionPrediction>> buildingActions;
    public Dictionary<int, List<ActionPrediction>> npcActions;
    public List<ActionPrediction> environmentActions;
    public List<ActionPrediction> selfActions;
    public ActionFrontier(DataController dataController)
    {
        buildingActions = new Dictionary<int, List<Action>>();
        List<int> buildingIDs = new List<int>(dataController.BuildingStorage.Keys);
        foreach(int id in buildingIDs)
        {
            buildingActions[id] = new List<Action>();
        }

        npcActions = new Dictionary<int, List<Action>>();
        List<int> npcIDs = new List<int>(dataController.NPCStorage.Keys);
        foreach(int id in npcIDs)
        {
            npcActions[id] = new List<Action>();
        }

        selfActions = new List<Actions>();
        environmentActions = new List<Actions>();
    }
}