using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public static class SimulationController
{
    private static IDataContainer dataController => DomainContext.DataController;

    private static float startTime;
    private static float currentTime;
    private static float endTime;
    private static DataControllerSnapshot domain;

    //Menu progress reporting
    public static event Action<SimulationProgress> OnProgress;
    private static void ReportProgress(float progress, string message)
    {
        OnProgress?.Invoke(new SimulationProgress(progress, message));
    }

    public static SimulationPlot initiateSimulationPlot(float simulationTime)
    {
        DataControllerSnapshot dataSnapshot = new DataControllerSnapshot(); //copy game state
        DomainContext.DataController = dataSnapshot;

        SimulationPlot simPlot = new SimulationPlot(dataSnapshot); //create simulation plot

        simPlot.startTime = dataSnapshot.worldManager.gameTime;
        simPlot.endTime = dataSnapshot.worldManager.gameTime + simulationTime;
        simPlot.runTime = simPlot.startTime;

        return simPlot;
    }

    public static float calcFinishPerc(ActionInfoWrapper action, NPCData agent)
    {
        if(action.energyToComplete > agent.stats.energy) return agent.stats.energy / action.energyToComplete;
        else return 100f;
    }

    public static IEnumerator plotSimulationRoutine(SimulationPlot simPlot)
    {
        domain = simPlot.domain; //set the data controller snapshot
        startTime = simPlot.startTime; //simulation start time
        endTime = simPlot.endTime; //simulation end time
        currentTime = startTime; //simulation current time

        ReportProgress(10.5f, "Preparing simulation plot...");
        yield return null;
        
        //creates action frontier AND sets DomainContext.DataController for actions

        //iterates over all agents and produces their potential actions at this point
        ActionFrontier actionFrontier = new ActionFrontier(domain);
        PriorityQueue<SimulationActionWrapper, float> actionQueue = new PriorityQueue<SimulationActionWrapper, float>();

        ReportProgress(12f, "Creating initial action plans");
        yield return null;

        //create initial actions
        foreach(int agentID in domain.NPCStorage.Keys)
        {
            ActionInfoWrapper bestAction = actionFrontier.getBestAction(domain.NPCStorage[agentID]); //get the best action for this agent

            if(bestAction.timeToComplete + startTime > endTime) continue; //if this action couldn't be performed in specified time
            
            float finishPerc = calcFinishPerc(bestAction, domain.NPCStorage[agentID]); //calculate at what percentage this should finish (energy constraints)
            SimulationActionWrapper thisAction = new SimulationActionWrapper(bestAction, 
                                                                            startTime, //when the action starts
                                                                            startTime + (bestAction.timeToComplete * finishPerc), //when the action should finish
                                                                            finishPerc //the percentage of the action that'll be completed
                                                                            ); //store as sim action

            simPlot.agentActiveActions[agentID] = thisAction; //set this agents current action
            actionQueue.Enqueue(thisAction, thisAction.endTime); //store the action in order of its end time
        }

        ReportProgress(12f, "Beginning plot clock...");
        yield return null;

        float simTotalTime = endTime - startTime;
        //simulation step
        while(currentTime < endTime)
        {
            if(currentTime % 1f == 0f)
            {
                ReportProgress(((currentTime - startTime)/simTotalTime)*88f + 12f, "Performing Actions at time: "+currentTime.ToString("00.00"));
                yield return null;
            }

            /*++++++++++PERFORM ANY ACTIONS THAT NEED TO OCCUR++++++++++*/
            while(actionQueue.TryPeek(out SimulationActionWrapper _, out float peekTime)) //while there are things in the queue
            {
                if(peekTime > currentTime) break; //if this action won't finish yet, then we've reached the end of actions that we need to perform

                else //we can perform this action
                {
                    if(!actionQueue.TryDequeue(out SimulationActionWrapper simAction, out float endTime)) continue; //get the next action
                    if(simAction.endTime != endTime) continue; //if the two times are different then this action has changed priority position

                    domain.worldManager.gameTime = currentTime; //set world time to be current time 

                    /*==========ACTION PERFORMING==========*/
                    IAction thisAction = simAction.info.action;
                    thisAction.reloadAction(simAction.info);
                    thisAction.performAction(simAction.percentComplete);

                    NPCData agent = domain.NPCStorage[thisAction.currentActor];

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
                                if(actWrap.info == simAction.info) simPlot.buildingActiveActions[recID].RemoveAt(iteration); //if this is the action we've just performed then remove it
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
                                float currentDuration = currentTime - actWrap.startTime;

                                float hundredPercDuration = (100f * originalDuration) / actWrap.percentComplete;

                                actWrap.percentComplete = (currentDuration / hundredPercDuration) * 100f;
                                actWrap.endTime = currentTime;

                                actionQueue.Enqueue(actWrap, currentTime); 
                            }
                        }
                    }

                    simPlot.agentActiveActions[thisAction.currentActor] = null; //show that this agent is currently not doing anything
                    simPlot.inactiveAgents.Add(thisAction.currentActor); //add the agent as inactive
                    simPlot.plottedActions.EnqueueBack(simAction); //add this action to the final action plot
                }
            }

            if(currentTime % 1f == 0f)
            {
                ReportProgress(((currentTime - startTime)/simTotalTime)*88f + 12f, "Plotting Actions at time: "+currentTime.ToString("00.00"));
                yield return null;
            }

            /*++++++++++GET NEW ACTIONS++++++++++*/
            foreach(int agentID in simPlot.inactiveAgents)
            {
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

            currentTime += 0.1f;
        }

        ReportProgress(100f, "Plotting Complete.");

        Debug.Log("Plotted Action number is "+simPlot.plottedActions.Count.ToString());
    }

    public static IEnumerator runPlotToTime(SimulationPlot simPlot, float plotProgressTime)
    {
        if(DomainContext.DataController is DataController dc) Debug.Log("Plot running on DataController instance.");
        else Debug.Log("Plot not running on DataController instance.");

        int totalActionNum = simPlot.plottedActions.Count;
        int currentActionNum = 0;

        float currTime = simPlot.startTime;

        //we either track by time or by actions complete
        bool wholeSim = false;
        if(plotProgressTime == simPlot.endTime) wholeSim = true;
        if(wholeSim) ReportProgress(0f, "Beginning run of complete simulation plot.");
        else ReportProgress(0f, "Begging run of simulation plot to time "+plotProgressTime.ToString("0.00"));
        yield return null;

        if((!wholeSim) && plotProgressTime <= simPlot.runTime) {
            simPlot.runComplete = true;
            yield break;
        }

        plotProgressTime = Mathf.Min(simPlot.endTime, plotProgressTime); //we run the simulation to the specified time or the plot endTime

        DEQueue<SimulationActionWrapper> actionQueue = simPlot.plottedActions;
        while(actionQueue.TryPeekFront(out SimulationActionWrapper peekAct)) //while there are actions in the queue
        {
            if(peekAct.endTime > plotProgressTime) break; //if the actions end time is later than our final time, then we don't need to perform anymore
            else
            {
                currentActionNum++;
                if(!actionQueue.TryDequeueFront(out SimulationActionWrapper actionWrap)) yield break;
                ActionInfoWrapper actionInfo = actionWrap.info;
                float perc = actionWrap.percentComplete;

                IAction action = actionInfo.action;

                if(wholeSim) ReportProgress((currentActionNum / totalActionNum)*100f, "Performing action ("+currentActionNum.ToString()+"/"+totalActionNum.ToString()+")");
                else ReportProgress((actionWrap.endTime - simPlot.runTime / plotProgressTime - simPlot.runTime)*100f, 
                                    "Performing action at time "+(actionWrap.endTime-simPlot.runTime).ToString("0.00")+"/"+(plotProgressTime - simPlot.runTime).ToString("0.00"));
                yield return null;

                dataController.World.gameTime = actionWrap.endTime;
                
                //perform the action with the percent completion
                action.reloadAction(actionInfo);
                action.performAction(perc);
            }
        }

        if(!actionQueue.TryPeekFront(out SimulationActionWrapper _)) simPlot.runComplete = true; //if there is nothing left in the simulation plot
        simPlot.runTime = plotProgressTime; //set the current progress of the plot
        ReportProgress(100f, "Simulation running complete.");
    }

    public static IEnumerator runFullPlot(SimulationPlot simPlot)
    {
        float runEndTime = simPlot.endTime;
        
        yield return runPlotToTime(simPlot, runEndTime);
    }
}