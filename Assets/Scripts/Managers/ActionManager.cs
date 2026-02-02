using UnityEngine;
using System.Collections.Generic;
public class ActionManager : MonoBehaviour
{
    public DataController dataController = DataController.Instance;

    public LoadActions()
    {
        List<BuildingAction> buildingActions = new Lists<BuildingAction>();
        List<NPCAction> npcActions = new List<NPCAction>();
        List<SelfAction> selfActions = new List<SelfAction>();
        List<EnvironmentAction> environmentActions = new List<EnvironmentAction>();
        Dictionary<string, ActionBase> ActionStorage = new Dictionart<string, ActionBase>();

        //Add each building Action

        //Add each npc Action

        //Add each self Action

        //Add each environment Action

        dataController.buildingActions = buildingActions;
        dataController.npcActions = npcActions;
        dataController.selfActions = selfActions;
        dataController.environmentActions = environmentActions;
        dataController.ActionStorage = ActionStorage;
    }
}