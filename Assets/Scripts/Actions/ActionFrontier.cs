using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ActionFrontier
{
    public ActionInfoWrapper bestAction;
    public Dictionary<int, List<ActionInfoWrapper>> buildingActions;
    public Dictionary<int, List<ActionInfoWrapper>> npcActions;
    public List<ActionInfoWrapper> environmentActions;
    public List<ActionInfoWrapper> selfActions;
    private IDataContainer dataController => DomainContext.DataController;
    public ActionFrontier(IDataContainer dataCont)
    {
        DomainContext.DataController = dataCont; //set the current action domain to to be whichever has been passed 

        buildingActions = new Dictionary<int, List<ActionInfoWrapper>>();
        List<int> buildingIDs = dataController.Buildings.Select(b => b.id).ToList();
        foreach(int id in buildingIDs)
        {
            buildingActions[id] = new List<ActionInfoWrapper>();
        }

        npcActions = new Dictionary<int, List<ActionInfoWrapper>>();
        List<int> agentIDs = dataController.Agents.Select(a => a.id).ToList();
        foreach(int id in agentIDs)
        {
            npcActions[id] = new List<ActionInfoWrapper>();
        }

        selfActions = new List<ActionInfoWrapper>();
        environmentActions = new List<ActionInfoWrapper>();
    }

    public void produceFrontier(IAgent performer)
    {
        List<int> agentKeys = dataController.Agents.Select(a => a.id).ToList();

        //compute each NPC action for this npc
        foreach(NPCAction agentAction in DataController.Instance.npcActions)
        {
           foreach(int agentKey in agentKeys)
            {
                if(!dataController.TryGetAgent(agentKey, out var agent)) continue;
                if(agent.id == performer.id) continue;

                ActionInfoWrapper info = agentAction.computeAction(performer, agent);
                npcActions[agent.id].Add(info);

                //check if this is the new best action
                if(info.estUtility > bestAction.estUtility) bestAction = info;
            } 
        }

        List<int> buildingKeys = dataController.Buildings.Select(b => b.id).ToList();
        //compute each Building action for this npc
        foreach(BuildingAction buildAct in DataController.Instance.buildingActions)
        {
            foreach(int buildingKey in buildingKeys)
            {
                if(!dataController.TryGetBuilding(buildingKey, out var building)) continue;
                
                ActionInfoWrapper info = buildAct.computeAction(performer, building);
                buildingActions[building.id].Add(info);

                if(info.estUtility > bestAction.estUtility) bestAction = info;
            }
        }

        foreach(SelfAction selfAct in DataController.Instance.selfActions)
        {
            ActionInfoWrapper info = selfAct.computeAction(performer, default);
            selfActions.Add(info);

            if(info.estUtility > bestAction.estUtility) bestAction = info;
        }

        foreach(EnvironmentAction envAct in DataController.Instance.environmentActions)
        {
            ActionInfoWrapper info = envAct.computeAction(performer, default);
            environmentActions.Add(info);

            if(info.estUtility > bestAction.estUtility) bestAction = info;
        }
        
    }
}