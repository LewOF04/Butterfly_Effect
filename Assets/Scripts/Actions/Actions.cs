using UnityEngine;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;
using System;

public abstract class Actions<T> : IAction
{
    protected Actions() { }
    protected Actions(char actionType) { }

    protected IDataContainer dataController => DomainContext.DataController;

    public abstract char actionType { get; }
    public abstract string name { get; }
    public abstract string baseDescription { get; }

    public int currentActor { get; set; } = -1;
    public int receiver { get; set; } = -1;
    public float timeToComplete { get; set; } = -1f;
    public float energyToComplete { get; set; } = -1f;
    public bool known { get; set; } = false;
    public float estUtility { get; set; } = -1f;
    public float actUtility { get; set; } = -1f;
    public float estSuccess { get; set; } = -1f;
    public float actSuccess { get; set; } = -1f;
    public virtual bool interrupts { get; set; } = false; //whether or not this action would interrupt an npc receiver

    [Range(0,24)] protected abstract float baseTime { get; } //the time it would take for a completely "normal" npc to complete
    [Range(0,100)] protected abstract float baseEnergy { get; } //the energy it would take for a completely "normal" npc to complete
    [Range(0,100)] protected abstract float complexity { get; } //how complex this action is to complete
    [Range(0,100)] protected abstract float baseUtility { get; } //how inheritly useful the action is
    protected abstract List<int> utilityPosTraits { get; } //the traits that will have an positive impact on perceived utility
    protected abstract List<int> utilityNegTraits { get; } //the traits that will have an negative impact on perceived utility
    protected abstract List<int> successPosTraits { get; }//the traits that will have a positive impact on the action success
    protected abstract List<int> successNegTraits { get; } //the traits that will have a negative impact on the action success
    protected abstract float computeUtility(IAgent performer, T receiver); //compute the empirical utility of the action 
    protected abstract float estimateUtility(IAgent performer, T receiver); //compute the performers perceived utility of the action
    protected abstract float computeSuccess(IAgent performer, T receiver); //computer the likelihood this action will be a success
    protected abstract float estimateSuccess(IAgent performer, T receiver); //compute the estimated chance this action will be a succss from the performers perspective
    protected abstract float getTimeToComplete(IAgent performer, T receiver); //calculate how much time it would take for the NPC to complete this action
    protected abstract float getEnergyToComplete(IAgent performer, T receiver); //calculate how much energy it would take the NPC to compelete this action
    protected abstract void innerPerformAction(float percentComplete); //make the changes of the action, with results affected by percentage completed
    public void performAction(float percentComplete)
    {
        if(!dataController.TryGetAgent(currentActor, out var thisAgent)) return;

        float energyPerc = (energyToComplete / thisAgent.stats.energy) * 100; //percent of action completed within energy

        innerPerformAction(Mathf.Min(energyPerc, percentComplete)); //perform the action with whatever perc would've run out first
    }
    
    //check that this action would be known to the NPC
    protected virtual bool isKnown(IAgent performer)
    {
        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        if(this is NPCAction || this is SelfAction || this is EnvironmentAction){
            List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
            foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;
        } 
        
        else if (this is BuildingAction)
        {
            List<BuildingEvent> buildingEvents = dataController.buildingEventsPerNPCStorage[performer.id];
            foreach(BuildingEvent thisEvent in buildingEvents) if(thisEvent.actionName == name) return true;
        }

        return false;
    } 

    public ActionInfoWrapper computeAction(IAgent performer, T receiver)
    {
        resetAction();
        currentActor = performer.id;

        this.receiver = receiver switch
        {
            IAgent npc         => npc.id,
            IBuilding build  => build.id,
            _               => -1
        };

        energyToComplete = getEnergyToComplete(performer, receiver);
        timeToComplete = getTimeToComplete(performer, receiver);
        known = isKnown(performer);
        actUtility = computeUtility(performer, receiver);
        estUtility = estimateUtility(performer, receiver);
        actSuccess = computeSuccess(performer, receiver);
        estSuccess = estimateSuccess(performer, receiver);

        return new ActionInfoWrapper(performer.id, this.receiver, timeToComplete, energyToComplete, known, estUtility, actUtility, estSuccess, actSuccess, this);
    }

    //reload the action object with the information from a previously computed info wrapper
    public void reloadAction(ActionInfoWrapper info)
    {
        resetAction();
        currentActor = info.currentActor;
        receiver = info.receiver;
        timeToComplete = info.timeToComplete;
        energyToComplete = info.energyToComplete;
        known = info.known;
        estUtility = info.estUtility;
        actUtility = info.actUtility;
        estSuccess = info.estSuccess;
        actSuccess = info.actSuccess;
    }

    /*
    Reset the action information to create new action instance
    */
    public void resetAction()
    {
        currentActor = -1;
        receiver = -1;
        timeToComplete = -1f;
        energyToComplete = -1f;
        known = false;
        estUtility = -1f;
        actUtility = -1f;
        estSuccess = -1f;
        actSuccess = -1f;
    }
}

public abstract class BuildingAction : Actions<IBuilding>
{
    public BuildingAction(char actionType = 'B') : base(actionType){}
}
public abstract class NPCAction : Actions<IAgent>
{
    public NPCAction(char actionType = 'N') : base(actionType){}
}
public abstract class SelfAction : Actions<NoTarget>
{
    public SelfAction(char actionType = 'S') : base(actionType){}
}
public abstract class EnvironmentAction : Actions<NoTarget>
{
    public EnvironmentAction(char actionType = 'E') : base(actionType){}
}

public interface IAction
{
    char actionType {get;} //action type ("N" = NPC, "B" = Building, "E" = Environment, "S" = Self, "Y" = System)
    string name {get;}
    string baseDescription {get;}

    int currentActor { get; set; }
    int receiver { get; set; }
    float timeToComplete { get; set; }
    float energyToComplete { get; set; }
    bool known { get; set; }
    float estUtility { get; set; }
    float actUtility { get; set; }
    float estSuccess { get; set; }
    float actSuccess { get; set; }
    bool interrupts { get; set; }

    void performAction(float percentComplete);
    void reloadAction(ActionInfoWrapper info);
    void resetAction();
}

public interface IAction<T> : IAction
{
    ActionInfoWrapper computeAction(IAgent performer, T receiver);

    float computeUtility(IAgent performer, T receiver);
    float estimateUtility(IAgent performer, T receiver);
    float computeSuccess(IAgent performer, T receiver);
    float estimateSuccess(IAgent performer, T receiver);
    float getTimeToComplete(IAgent performer, T receiver);
    float getEnergyToComplete(IAgent performer, T receiver);
    bool isKnown(IAgent performer);
    void innerPerformAction(float percentComplete); 
}