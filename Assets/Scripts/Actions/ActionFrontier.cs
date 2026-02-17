using UnityEngine;
using System.Collections.Generic;

public class ActionFrontier
{
    public ActionInfoWrapper bestAction;
    public Dictionary<int, List<ActionInfoWrapper>> buildingActions;
    public Dictionary<int, List<ActionInfoWrapper>> npcActions;
    public List<ActionInfoWrapper> environmentActions;
    public List<ActionInfoWrapper> selfActions;
    private DataController dataController;
    public ActionFrontier(DataController dataCont)
    {
        dataController = dataCont;

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

    public void produceFrontier(NPC performer)
    {
        Dictionary<int, NPC> npcStorage = dataController.NPCStorage;
        List<int> npcKeys = new List<int>(npcStorage.Keys);
        //compute each NPC action for this npc
        foreach(NPCAction npcAct in dataController.npcActions)
        {
           foreach(int npcKey in npcKeys)
            {
                NPC npc = npcStorage[npcKey];
                if(npc.id == performer.id) continue;

                ActionInfoWrapper info = npcAct.computeAction(performer, npc);
                npcActions[npc.id].Add(info);

                //check if this is the new best action
                if(info.estUtility > bestAction.estUtility) bestAction = info;
            } 
        }

        Dictionary<int, Building> buildingStorage = dataController.BuildingStorage;
        List<int> buildingKeys = new List<int>(buildingStorage.Keys);
        //compute each Building action for this npc
        foreach(BuildingAction buildAct in dataController.buildingActions)
        {
            foreach(int buildingKey in buildingKeys)
            {
                Building building = buildingStorage[buildingKey];
                
                ActionInfoWrapper info = buildAct.computeAction(performer, building);
                buildingActions[building.id].Add(info);

                if(info.estUtility > bestAction.estUtility) bestAction = info;
            }
        }

        foreach(SelfAction selfAct in dataController.selfActions)
        {
            ActionInfoWrapper info = selfAct.computeAction(performer, default);
            selfActions.Add(info);

            if(info.estUtility > bestAction.estUtility) bestAction = info;
        }

        foreach(EnvironmentAction envAct in dataController.environmentActions)
        {
            ActionInfoWrapper info = envAct.computeAction(performer, default);
            environmentActions.Add(info);

            if(info.estUtility > bestAction.estUtility) bestAction = info;
        }
        
    }
}