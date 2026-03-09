using UnityEngine;
using System.Collections.Generic;
using System;

public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance;
    private IDataContainer dataController => DomainContext.DataController;

    private float startTime;
    private float currentTime;
    private float endTime;
    private DataControllerSnapshot domain;

    public SimulationPlot initiateSimulationPlot(float simulationTime)
    {
        DataControllerSnapshot dataSnapshot = new DataControllerSnapshot(); //copy game state
        DomainContext.DataController = dataSnapshot;

        SimulationPlot simPlot = new SimulationPlot(dataSnapshot); //create simulation plot

        simPlot.startTime = snapshot.World.gameTime;
        simPlot.endTime = snapshot.World.gameTime + simulationTime;
        simPlot.runTime = simPlot.startTime;

        return simPlot;
    }

    public bool isActiveTime(float agentTimeLeft, float dayTime)
    {
        //dayTime = how many hours into the day
        float agentTime = 24f - agentTimeLeft; //time between 0f-24f which is their time left in the day
        
        if(agentTime > dayTime) return false;
        return true;
    }

    public float calcFinishPerc(ActionInfoWrapper action, NPCData agent)
    {
        if(action.energyToComplete > agent.stats.energy) return agent.stats.energy / action.energyToComplete;
        else return 100f;
    }



    public SimulationPlot plotSimulation(SimulationPlot simPlot)
    {
        domain = simPlot.domain;
        startTime = simPlot.startTime; //simulation start time
        endTime = simPlot.endTime; //simulation end time
        currentTime = startTime; //simulation current time

        //creates action frontier AND sets DomainContext.DataController for actions
        ActionFrontier actionFrontier = new ActionFrontier(domain);
        PriorityQueue<SimulationActionWrapper, float> actionQueue = new PriorityQueue<SimulationActionWrapper, float>();

        float dayTime = startTime % 24f; //are we progressed into this day at all

        //create initial actions
        foreach(int agentID in domain.NPCStorage.Keys)
        {
            if(!isActiveTime(domain.NPCStorage[agentID].timeLeft, dayTime)) //if the agents time is later than the current time, skip until their time
            {
                simPlot.inactiveAgents.Add(agentID);
                continue;
            }

            ActionInfoWrapper bestAction = actionFrontier.getBestAction(domain.NPCStorage[agentID]); //get the best action for this agent
            
            float finishPerc = calcFinishPerc(bestAction, domain.NPCStorage[agentID]); //calculate at what percentage this should finish (energy constraints)
            SimulationActionWrapper thisAction = new SimulationActionWrapper(bestAction, 
                                                                            startTime, //when the action starts
                                                                            startTime + (bestAction.timeToComplete * finishPerc), //when the action should finish
                                                                            finishPerc //the percentage of the action that'll be completed
                                                                            ); //store as sim action

            simPlot.agentActiveActions[agentID] = thisAction; //set this agents current action
            actionQueue.Enqueue(thisAction, thisAction.endTime); //store the action in order of its end time
        }

        
        //simulation step
        while(currentTime < endTime)
        {
            /*++++++++++PERFORM ANY ACTIONS THAT NEED TO OCCUR++++++++++*/
            while(actionQueue.TryPeek(out SimulationActionWrapper _, out float peekTime)) //while there are things in the queue
            {
                if(peekTime > currentTime) break; //if this action won't finish yet, then we've reached the end of actions that we need to perform

                else //we can perform this action
                {
                    actionQueue.Dequeue(out SimulationActionWrapper simAction, out float endTime); //get the next action
                    if(simAction.endTime != endTime) continue; //if the two times are different then this action has changed priority position

                    /*==========ACTION PERFORMING==========*/
                    Action thisAction = simAction.info.action;
                    thisAction.reloadAction(simAction.info);
                    thisAction.performAction(simAction.percentComplete);

                    NPCData agent = domain.NPCStorage[thisAction.currentActor];
                    agent.timeLeft -= thisAction.timeToComplete * (simAction.percentComplete / 100f); //change agents time
                    if(agent.timeLeft < 0f) agent.timeLeft = 24f + agent.timeLeft; //move time over to next day

                    /*==========INTERRUPTION HANDLING==========*/
                    //if this action being performed will interrupt other actions
                    if (thisAction.interrupts)
                    {
                        int recID = thisAction.receiver; //who is the action receiver

                        //if this is an action being performed to a building
                        if(thisAction is BuildingAction buildAct)
                        {
                            int iteration = 0;

                            //interrupt all actions currently being performed to this building
                            foreach(SimulationActionWrapper actWrap in simPlot.buildingActiveActions[recID])
                            {
                                if(actWrap.info == simAction.info) simPlot.buildingActiveActions.RemoveAt(iteration); //if this is the action we've just performed then remove it
                                else
                                {
                                    float originalDuration = actWrap.endTime - actWrap.startTime; //how long this action should have taken
                                    float currentDuration = currentTime - actWrap.startTime; //how long this action will now take

                                    float hundredPercDuration = (100f * originalDuration) / actWrap.percentComplete; //how long it would take if 100% completion

                                    actWrap.percentComplete = (currentDuration / hundredPercDuration) * 100f; //what percent completion it will now take
                                    actWrap.endTime = currentTime; //the time this action has now ended

                                    actionQueue.Enqueue(actWrap, currentTime); //re-enqueue this action 
                                }
                                iteration++;
                            }
                        }

                        //if this is an action being performed to an agent
                        if(thisAction is NPCAction agentAct)
                        {
                            SimulationActionWrapper actWrap = simPlot.agentActiveActions[recID]; //get the current action of the interrupted agent
                            if(actWrap != null) //if this agent is performing an action
                            {
                                float originalDuration = actWrap.endTime - actWrap.startTime;
                                float currentDuration = currentTime = actWrap.startTime;

                                float hundredPercDuration = (100f * originalDuration) / actWrap.percentComplete;

                                actWrap.percentComplete = (currentDuration / hundredPercDuration) * 100f;
                                actWrap.endTime = currentTime;

                                actionQueue.Enqueue(actWrap, currentTime); 
                            }
                        }
                    }

                    simPlot.agentActiveActions[thisAction.currentActor] = null;
                    simPlot.inactiveAgents.Add(thisAction.currentActor); //add the agent as inactive
                    simPlot.plottedActions.EnqueueBack(simAction); //add this action to the final action plot
                }
            }

            /*++++++++++GET NEW ACTIONS++++++++++*/
            foreach(int agentID in simPlot.inactiveAgents)
            {
                if(!isActiveTime(domain.NPCStorage[agentID].timeLeft, dayTime)) continue;//if the agent's time is later than the current time, skip 

                NPCData agent = domain.NPCStorage[agentID];
                ActionInfoWrapper bestAction = actionFrontier.getBestAction(agent); //get the best action for this agent
            
                float finishPerc = calcFinishPerc(bestAction, agent); //calculate at what percentage this should finish (energy constraints)
                SimulationActionWrapper thisAction = new SimulationActionWrapper(bestAction, 
                                                                                currentTime, //when the action starts
                                                                                currentTime + (bestAction.timeToComplete * finishPerc), //when the action should finish
                                                                                finishPerc //the percentage of the action that'll be completed
                                                                                ); //store as sim action

                simPlot.agentActiveActions[agentID] = thisAction; //set this agents current action
                actionQueue.Enqueue(thisAction, thisAction.endTime); //store the action in order of its end time
            }

            currentTime += 0.01f;
            dayTime = currentTime % 24f; //current time of day
        }

        return simPlot;
    }

    public SimulationPlot runPlotToTime(SimulationPlot simPlot, float plotProgressTime)
    {
        if(DomainContext.DataController is DataController dc) Debug.Log("Plot running on DataController instance.");
        else Debug.Log("Plot not running on DataController instance.");

        if(plotProgressTime <= simPlot.runTime) {
            simPlot.runComplete = true;
            return simPlot; //if we've reached the end then return
        }

        plotProgressTime = Mathf.Min(simPlot.endTime, plotProgressTime); //we run the simulation to the specified time or the plot endTime

        DEQueue<SimulationActionWrapper> actionQueue = simPlot.plottedActions;
        while(actionQueue.TryPeekFront(out SimulationActionWrapper peekAct)) //while there are actions in the queue
        {
            if(peekAct.endTime > plotProgressTime) break; //if the actions end time is later than our final time, then we don't need to perform anymore
            else
            {
                if(!actionQueue.TryDequeueFront(out SimulationActionWrapper actionWrap)) return;
                ActionInfoWrapper actionInfo = actionWrap.info;
                float perc = actionWrap.percentComplete;

                Action action = actionInfo.action;

                action.reloadAction(actionInfo);
                action.performAction(perc);
            }
        }

        if(!actionQueue.TryPeekFront(out SimulationActionWrapper _)) simPlot.runComplete = true;
        simPlot.runTime = plotProgressTime;
        
        return simPlot;
    }

    public void runFullPlot(SimulationPlot simPlot)
    {
        float runEndTime = simPlot.endTime;
        
        SimulationPlot finalPlot = runPlotToTime(simPlot, runEndTime);
    }
}