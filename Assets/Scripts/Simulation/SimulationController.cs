using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

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

        Debug.Log("STATE COPIED\n startTime = "+simPlot.startTime.ToString("0.00")+"\nendTime = "+simPlot.endTime.ToString("0.00")+"\nrunTime = "+simPlot.runTime.ToString("0.00"));

        return simPlot;
    }

    public static float calcFinishPerc(ActionInfoWrapper action, NPCData agent)
    {
        if(action.energyToComplete > agent.stats.energy) return (agent.stats.energy / action.energyToComplete)*100f;
        else return 100f;
    }

    private static void plotInitialActions(ActionFrontier actionFrontier, PriorityQueue<SimulationActionWrapper, float> actionQueue, SimulationPlot simPlot)
    {
        //create initial actions
        foreach(int agentID in domain.NPCStorage.Keys)
        {
            Debug.Log("Initial Action of Agent: "+agentID.ToString());
            NPCData agent = domain.NPCStorage[agentID];
            if(!agent.isAlive) continue;

            ActionInfoWrapper bestAction = actionFrontier.getBestAction(agent); //get the best action for this agent
            Debug.Log("Best action: "+bestAction.action.name);

            if(bestAction.timeToComplete + startTime > endTime) continue; //if this action couldn't be performed in specified time
            Debug.Log("Best Action time to complete: "+bestAction.timeToComplete.ToString()+"\n\'StartTime\': "+startTime.ToString()+"\n\'End Time\': "+endTime.ToString());
            
            float finishPerc = calcFinishPerc(bestAction, domain.NPCStorage[agentID]); //calculate at what percentage this should finish (energy constraints)
            SimulationActionWrapper thisAction = new SimulationActionWrapper(bestAction, 
                                                                            startTime, //when the action starts
                                                                            startTime + (bestAction.timeToComplete * (finishPerc/100)), //when the action should finish
                                                                            finishPerc //the percentage of the action that'll be completed
                                                                            ); //store as sim action

            simPlot.agentActiveActions[agentID] = thisAction; //set this agents current action
            actionQueue.Enqueue(thisAction, thisAction.endTime); //store the action in order of its end time
            Debug.Log("Enqueued action for agent: "+agentID+"\nactionQueue Count: "+actionQueue.Count.ToString());
        }
    }

    private static void checkAgentDeaths(PriorityQueue<SimulationActionWrapper, float> actionQueue, SimulationPlot simPlot)
    {
        //check on agent death
        List<int> deathKeys = new List<int>(domain.NPCStorage.Keys);
        foreach(int deathID in deathKeys)
        {
            NPCData agent = domain.NPCStorage[deathID];
            if(!agent.isAlive) continue; //if the agent is already dead, then ignore

            if(agent.stats.condition <= 0) //if they should be dead
            {
                //get all agentIDs from agent active actions that is not null
                List<int> agentIDs = simPlot.agentActiveActions.Where(kvp => kvp.Value != null).Select(kvp => kvp.Key).ToList();

                //finish any actions which involve the dead agent
                foreach(int id in agentIDs)
                {
                    SimulationActionWrapper simAction = simPlot.agentActiveActions[id];
                    if(simAction.info.receiver == agent.id || simAction.info.currentActor == agent.id) //if the performer or receiver is the dead agent
                    {
                        float originalDuration = simAction.endTime - simAction.startTime; //how long this action should have taken
                        float currentDuration = currentTime - simAction.startTime; //how long this action will now take

                        float hundredPercDuration = (100f * originalDuration) / simAction.percentComplete; //how long it would take if 100% completion

                        simAction.percentComplete = (currentDuration / hundredPercDuration) * 100f; //what percent completion it will now take
                        simAction.endTime = currentTime; //the time this action has now ended

                        actionQueue.Enqueue(simAction, currentTime);
                    }
                }

                //calculate death impact
                AgentDeathInfo deathInfo = SystemAction.calcAgentDeath(agent.id, currentTime); 
                simPlot.plottedActions.EnqueueBack(deathInfo); 
                deathInfo.performUpdate(); 
            }
        }
    }

    private static void performQueuedActions(PriorityQueue<SimulationActionWrapper, float> actionQueue, SimulationPlot simPlot)
    {
        /*++++++++++PERFORM ANY ACTIONS THAT NEED TO OCCUR++++++++++*/
        while(actionQueue.TryPeek(out SimulationActionWrapper _, out float peekTime)) //while there are things in the queue
        {
            Debug.Log("Peek time: "+peekTime.ToString()+"> "+currentTime.ToString());
            if(peekTime > currentTime) return; //if this action won't finish yet, then we've reached the end of actions that we need to perform
            else //we can perform this action
            {
                Debug.Log("After peek time");
                if(!actionQueue.TryDequeue(out SimulationActionWrapper simAction, out float actionEndTime)) continue; //get the next action
                Debug.Log("Performing Action "+simAction.info.action.name);
                if(simAction.endTime != actionEndTime) continue; //if the two times are different then this action has changed priority position

                domain.worldManager.gameTime = currentTime; //set world time to be current time 

                /*==========ACTION PERFORMING==========*/
                IAction thisAction = simAction.info.action;
                NPCData agent = domain.NPCStorage[thisAction.currentActor];
                
                thisAction.reloadAction(simAction.info);
                thisAction.performAction(simAction.percentComplete);

                if(thisAction is HaveChild)
                {
                    foreach(IAgent newAgent in dataController.Agents)
                    {
                        try{SimulationActionWrapper testWrap = simPlot.agentActiveActions[newAgent.id];}
                        catch
                        {
                            simPlot.agentActiveActions[newAgent.id] = null;
                            if(!simPlot.inactiveAgents.Contains(newAgent.id)) simPlot.inactiveAgents.Add(newAgent.id);
                        }
                    }
                }

                Debug.Log("Action performed");

                /*==========INTERRUPTION HANDLING==========*/
                //if this action being performed will interrupt other actions
                if (thisAction.interrupts)
                {
                    handleInterruption(thisAction, simPlot, actionQueue, simAction);
                }

                simPlot.agentActiveActions[thisAction.currentActor] = null; //show that this agent is currently not doing anything
                if (!simPlot.inactiveAgents.Contains(thisAction.currentActor)) simPlot.inactiveAgents.Add(thisAction.currentActor); //add the agent as inactive
                simPlot.plottedActions.EnqueueBack(simAction); //add this action to the final action plot
            }
        }
    }

    private static void handleInterruption(IAction thisAction, SimulationPlot simPlot, PriorityQueue<SimulationActionWrapper, float> actionQueue, SimulationActionWrapper simAction)
    {
        Debug.Log("Is interrupting");
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

    private static void performHourlyUpdates(SimulationPlot simPlot)
    {
        //run passive changes to agents and buildings (run once an hour)
        List<int> agentKeys = new List<int>(domain.NPCStorage.Keys);
        foreach(int agentKey in agentKeys)
        {
            NPCData agent = domain.NPCStorage[agentKey];
            if(!agent.isAlive) continue;
            AgentUpdateInfo updateInfo = SystemAction.calcAgentUpdate(agent.id, currentTime);
            simPlot.plottedActions.EnqueueBack(updateInfo);
            updateInfo.performUpdate(); 
        }
        List<int> buildingKeys = new List<int>(domain.BuildingStorage.Keys);
        foreach(int buildingKey in buildingKeys)
        {
            BuildingData building = domain.BuildingStorage[buildingKey];
            BuildingUpdateInfo updateInfo = SystemAction.calcBuildingUpdate(building.id, currentTime);
            simPlot.plottedActions.EnqueueBack(updateInfo);
            updateInfo.performUpdate();
        }
    }

    private static void getNewActions(SimulationPlot simPlot, PriorityQueue<SimulationActionWrapper, float> actionQueue, ActionFrontier actionFrontier)
    {
        HistoryManager.Instance.deleteUnimportantEvents(); //re-evaluate memory importance and remove events that would no longer be remembered by the agent
        /*++++++++++GET NEW ACTIONS++++++++++*/
        for(int i = simPlot.inactiveAgents.Count - 1; i >= 0; i--)
        {
            int agentID = simPlot.inactiveAgents[i];
            Debug.Log("Getting new action for "+agentID.ToString());
            NPCData agent = domain.NPCStorage[agentID];
            if(!agent.isAlive) {Debug.Log("Dead: "+agentID.ToString()); simPlot.inactiveAgents.RemoveAt(i); continue;}
            ActionInfoWrapper bestAction = actionFrontier.getBestAction(agent); //get the best action for this agent
        
            float finishPerc = calcFinishPerc(bestAction, agent); //calculate at what percentage this should finish (energy constraints)
            SimulationActionWrapper thisAction = new SimulationActionWrapper(bestAction, 
                                                                            currentTime, //when the action starts
                                                                            currentTime + (bestAction.timeToComplete * (finishPerc/100)), //when the action should finish
                                                                            finishPerc //the percentage of the action that'll be completed
                                                                            ); //store as sim action

            simPlot.agentActiveActions[agentID] = thisAction; //set this agents current action
            simPlot.inactiveAgents.RemoveAt(i);
            actionQueue.Enqueue(thisAction, thisAction.endTime); //store the action in order of its end time
        }
    }

    private static void completeActionsLeft(PriorityQueue<SimulationActionWrapper, float> actionQueue, SimulationPlot simPlot)
    {
        while(actionQueue.TryDequeue(out SimulationActionWrapper simAction, out float actionEndTime))
        {
            if(simAction.endTime != actionEndTime) continue; //time has changed which means this is no longer a valid action (has been overwritten)

            IAction thisAction = simAction.info.action;
            NPCData agent = domain.NPCStorage[thisAction.currentActor];

            float originalDuration = simAction.endTime - simAction.startTime; //how long this action should have taken
            float currentDuration = currentTime - simAction.startTime; //how long this action will now take

            float hundredPercDuration = (100f * originalDuration) / simAction.percentComplete; //how long it would take if 100% completion

            simAction.percentComplete = (currentDuration / hundredPercDuration) * 100f; //what percent completion it will now take
            simAction.endTime = currentTime; //the time this action has now ended

            thisAction.reloadAction(simAction.info);
            thisAction.performAction(simAction.percentComplete);

            simPlot.agentActiveActions[thisAction.currentActor] = null;
            if (!simPlot.inactiveAgents.Contains(thisAction.currentActor)) simPlot.inactiveAgents.Add(thisAction.currentActor); //add the agent as inactive
            simPlot.plottedActions.EnqueueBack(simAction); //add this action to the final action plot
        }    
    }

    public static IEnumerator plotSimulationRoutine(SimulationPlot simPlot)
    {
        Debug.Log("Entered simulation plot routine");
        domain = simPlot.domain; //set the data controller snapshot
        startTime = simPlot.startTime; //simulation start time
        endTime = simPlot.endTime; //simulation end time
        currentTime = startTime; //simulation current time

        ReportProgress(10.5f, "Preparing simulation plot...");
        yield return null;

        //iterates over all agents and produces their potential actions at this point
        ActionFrontier actionFrontier = new ActionFrontier(domain);
        PriorityQueue<SimulationActionWrapper, float> actionQueue = new PriorityQueue<SimulationActionWrapper, float>();

        ReportProgress(12f, "Creating initial action plans");
        yield return null;

        Debug.Log("Simulation plot routine before initial actions.");

        HistoryManager.Instance.deleteUnimportantEvents(); //re-evaluate memory importance and remove events that would no longer be remembered by the agent

        //plot the intial actions for each agent
        plotInitialActions(actionFrontier, actionQueue, simPlot);

        ReportProgress(12f, "Beginning plot clock...");
        yield return null;

        float simTotalTime = endTime - startTime;
        //simulation step
        while(currentTime <= endTime)
        {
            Debug.Log("Start of while loop.\n Current Time: "+currentTime.ToString("0.00")+"\nEnd Time: "+endTime.ToString("0.00"));
            if(Mathf.Abs(currentTime % 1f) < 0.001f)
            {
                ReportProgress(((currentTime - startTime)/simTotalTime)*88f + 12f, "Performing Actions at time: "+currentTime.ToString());
                yield return null;
            }

            checkAgentDeaths(actionQueue, simPlot);

            Debug.Log("Before peek action queue size: "+actionQueue.Count.ToString());
            Debug.Log("Truth check "+actionQueue.TryPeek(out SimulationActionWrapper _, out float _));
            performQueuedActions(actionQueue, simPlot);

            //update loading screen once an hour and perform hourly updates
            if(Mathf.Abs(currentTime % 1f) < 0.001f)
            {
                //update loading screen
                ReportProgress(((currentTime - startTime)/simTotalTime)*88f + 12f, "Plotting Actions at time: "+currentTime.ToString("0.00"));
                yield return null;

                performHourlyUpdates(simPlot);
            }

            Debug.Log("Before getting new actions.\n Inactive agents = "+string.Join(", ", simPlot.inactiveAgents));
            getNewActions(simPlot, actionQueue, actionFrontier); //get new actions for any inactive agents

            currentTime += 0.1f;
            currentTime = Mathf.Round(currentTime * 10f) / 10f; //round to 1dp
            Debug.Log("Current time: "+currentTime.ToString());
            yield return new WaitForSeconds(0.0000000001f);
        }

        //update world and current time for end of simulation
        currentTime = endTime;
        domain.worldManager.gameTime = currentTime;

        completeActionsLeft(actionQueue, simPlot);

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

        DEQueue<ISimEvent> actionQueue = simPlot.plottedActions;
        while(actionQueue.TryPeekFront(out ISimEvent peekAct)) //while there are actions in the queue
        {
            if(peekAct.eventTime > plotProgressTime) break; //if the actions end time is later than our final time, then we don't need to perform anymore
            else
            {
                currentActionNum++;
                if(!actionQueue.TryDequeueFront(out ISimEvent simEvent)) yield break;

                if(simEvent is SimulationActionWrapper actionWrap)
                {
                    ActionInfoWrapper actionInfo = actionWrap.info;
                    float perc = actionWrap.percentComplete;

                    IAction action = actionInfo.action;

                    if(currentActionNum % 10 == 0 || currentActionNum == totalActionNum)
                    {
                        if(wholeSim) ReportProgress((currentActionNum / totalActionNum)*100f, "Performing action ("+currentActionNum.ToString()+"/"+totalActionNum.ToString()+")");
                        else ReportProgress((actionWrap.endTime - simPlot.runTime / plotProgressTime - simPlot.runTime)*100f, 
                                            "Performing action at time "+(actionWrap.endTime-simPlot.runTime).ToString("0.00")+"/"+(plotProgressTime - simPlot.runTime).ToString("0.00"));
                        yield return null; 
                    }

                    dataController.World.gameTime = actionWrap.endTime;

                    //perform the action with the percent completion
                    action.reloadAction(actionInfo);
                    action.performAction(perc);  
                }
                if(simEvent is IUpdateInfo updateInfo)
                {
                    updateInfo.performUpdate();
                }
            }
        }

        if(!actionQueue.TryPeekFront(out ISimEvent _)) simPlot.runComplete = true; //if there is nothing left in the simulation plot
        simPlot.runTime = plotProgressTime; //set the current progress of the plot
        ReportProgress(100f, "Simulation running complete.");
    }

    public static IEnumerator runFullPlot(SimulationPlot simPlot)
    {
        float runEndTime = simPlot.endTime;
        
        yield return runPlotToTime(simPlot, runEndTime);
    }
}