using UnityEngine;
using System.Collections.Generic;

public class ActionFrontier
{
    public IActionBase bestAction;
    public Dictionary<int, List<ActionInfoWrapper>> buildingActions;
    public Dictionary<int, List<ActionInfoWrapper>> npcActions;
    public List<ActionInfoWrapper> environmentActions;
    public List<ActionInfoWrapper> selfActions;
    public ActionFrontier(DataController dataController)
    {
        buildingActions = new Dictionary<int, List<ActionInfoWrapper>>();
        List<int> buildingIDs = new List<int>(dataController.BuildingStorage.Keys);
        foreach(int id in buildingIDs)
        {
            buildingActions[id] = new List<ActionInfoWrapper>();
        }

        npcActions = new Dictionary<int, List<ActionInfoWrapper>>();
        List<int> npcIDs = new List<int>(dataController.NPCStorage.Keys);
        foreach(int id in npcIDs)
        {
            npcActions[id] = new List<ActionInfoWrapper>();
        }

        selfActions = new List<ActionInfoWrapper>();
        environmentActions = new List<ActionInfoWrapper>();
    }
}