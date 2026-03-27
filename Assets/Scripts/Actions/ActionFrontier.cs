using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ActionFrontier
{
    public ActionInfoWrapper bestAction;
    private bool bestActionSet = false;
    public Dictionary<int, List<ActionInfoWrapper>> buildingActions;
    public Dictionary<int, List<ActionInfoWrapper>> npcActions;
    public List<ActionInfoWrapper> environmentActions;
    public List<ActionInfoWrapper> selfActions;
    private IDataContainer dataController => DomainContext.DataController;
    public ActionFrontier(IDataContainer dataCont)
    {
        resetFrontier(dataCont);
    }

    public void resetFrontier(IDataContainer dataCont)
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

        bestActionSet = false;
        bestAction = default;
    }

    public ActionInfoWrapper getBestAction(IAgent performer)
    {
        return produceBestAction(performer);
    }

    private ActionInfoWrapper produceBestAction(IAgent performer)
    {
        resetFrontier(dataController);

        ActionInfoWrapper topAction = default;
        bool topActionSet = false;

        List<int> agentKeys = dataController.Agents.Select(a => a.id).ToList();

        //compute each NPC action for this npc
        foreach(NPCAction agentAction in DataController.Instance.npcActions)
        {
           foreach(int agentKey in agentKeys)
            {
                if(!dataController.TryGetAgent(agentKey, out var agent)) continue;
                if(agent.id == performer.id || !agent.isAlive) continue;

                ActionInfoWrapper info = agentAction.computeAction(performer, agent);

                //check if this is the new best action
                if(topActionSet == false || info.estUtility > topAction.estUtility) 
                {
                    topAction = info;
                    topActionSet = true;
                }
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

                if(topActionSet == false || info.estUtility > topAction.estUtility) 
                {
                    topAction = info;
                    topActionSet = true;
                }
            }
        }

        foreach(SelfAction selfAct in DataController.Instance.selfActions)
        {
            ActionInfoWrapper info = selfAct.computeAction(performer, default);

            if(topActionSet == false || info.estUtility > topAction.estUtility) 
            {
                topAction = info;
                topActionSet = true;
            }
        }

        foreach(EnvironmentAction envAct in DataController.Instance.environmentActions)
        {
            ActionInfoWrapper info = envAct.computeAction(performer, default);

            if(topActionSet == false || info.estUtility > topAction.estUtility) 
            {
                topAction = info;
                topActionSet = true;
            }
        }

        return topAction;
    }

    public void produceFrontier(IAgent performer)
    {
        resetFrontier(dataController);

        List<int> agentKeys = dataController.Agents.Select(a => a.id).ToList();

        //compute each NPC action for this npc
        foreach(NPCAction agentAction in DataController.Instance.npcActions)
        {
           foreach(int agentKey in agentKeys)
            {
                if(!dataController.TryGetAgent(agentKey, out var agent)) continue;
                if(agent.id == performer.id || !agent.isAlive) continue;

                ActionInfoWrapper info = agentAction.computeAction(performer, agent);
                npcActions[agent.id].Add(info);

                //check if this is the new best action
                if(bestActionSet == false || info.estUtility > bestAction.estUtility)
                {
                    bestActionSet = true;
                    bestAction = info;
                } 
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

                if(bestActionSet == false || info.estUtility > bestAction.estUtility)
                {
                    bestActionSet = true;
                    bestAction = info;
                } 
            }
        }

        foreach(SelfAction selfAct in DataController.Instance.selfActions)
        {
            ActionInfoWrapper info = selfAct.computeAction(performer, default);
            selfActions.Add(info);

            if(bestActionSet == false || info.estUtility > bestAction.estUtility)
            {
                bestActionSet = true;
                bestAction = info;
            } 
        }

        foreach(EnvironmentAction envAct in DataController.Instance.environmentActions)
        {
            ActionInfoWrapper info = envAct.computeAction(performer, default);
            environmentActions.Add(info);

            if(bestActionSet == false || info.estUtility > bestAction.estUtility)
            {
                bestActionSet = true;
                bestAction = info;
            } 
        }
    }
}