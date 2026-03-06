using UnityEngine;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;
using System;

public abstract class Action<T> : IAction
{
    protected Action() { }
    protected Action(char actionType) { }

    protected IDataContainer dataController => DomainContext.DataController;

    public abstract char actionType {get;}
    public virtual string name => GetType().Name;
    public abstract string baseDescription {get;}

    //stores for quick reference is talking about the same npc
    protected int currentActor = -1;
    protected int receiver = -1;
    protected float timeToComplete = -1f;
    protected float energyToComplete = -1f;
    protected bool? known = null;
    protected float estUtility = -1f;
    protected float actUtility = -1f;
    protected float estSuccess = -1f;
    protected float actSuccess = -1f;

    [Range(0,24)] protected abstract float baseTime {get;} //the time it would take for a completely "normal" npc to complete
    [Range(0,100)] protected abstract float baseEnergy {get;} //the energy it would take for a completely "normal" npc to complete
    [Range(0,100)] protected abstract float complexity {get;} //how complex this action is to complete
    [Range(0,100)] protected abstract float baseUtility {get;} //how inheritly useful the action is
    protected virtual bool interrupts => false; //whether or not this action would interrupt an npc receiver
    protected abstract List<int> utilityPosTraits {get;} //the traits that will have an positive impact on perceived utility
    protected abstract List<int> utilityNegTraits {get;} //the traits that will have an negative impact on perceived utility
    protected abstract List<int> successPosTraits {get;}//the traits that will have a positive impact on the action success
    protected abstract List<int> successNegTraits {get;} //the traits that will have a negative impact on the action success
    protected abstract float computeUtility(IAgent performer, T receiver); //compute the empirical utility of the action 
    protected abstract float estimateUtility(IAgent performer, T receiver); //compute the performers perceived utility of the action
    protected abstract float computeSuccess(IAgent performer, T receiver); //computer the likelihood this action will be a success
    protected abstract float estimateSuccess(IAgent performer, T receiver); //compute the estimated chance this action will be a succss from the performers perspective
    protected abstract float getTimeToComplete(IAgent performer, T receiver); //calculate how much time it would take for the NPC to complete this action
    protected abstract float getEnergyToComplete(IAgent performer, T receiver); //calculate how much energy it would take the NPC to compelete this action
    protected abstract void innerPerformAction(float percentComplete); //make the changes of the action, with results affected by percentage completed
    public string performAction(float percentComplete)
    {
        //calculate if the NPC would have run out of time or energy before completion
        if(!dataController.TryGetAgent(currentActor, out var thisAgent)) return "fail";

        float energyPerc = (energyToComplete / thisAgent.stats.energy) * 100; //percent of action completed within energy
        float timePerc = (timeToComplete / thisAgent.timeLeft) * 100; //percent of action completed within time

        innerPerformAction(Mathf.Min(energyPerc, timePerc, percentComplete)); //perform the action with whatever perc would've run out first
        
        //return string for whichever aspect was the finishing decider
        if(energyPerc == Mathf.Min(energyPerc, timePerc, percentComplete)) return "energy";
        else if(timePerc == Mathf.Min(energyPerc, timePerc, percentComplete)) return "time";
        else return "perc";
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
        known = null;
        estUtility = -1f;
        actUtility = -1f;
        estSuccess = -1f;
        actSuccess = -1f;
    }
}

public abstract class BuildingAction : Action<IBuilding>
{
    public BuildingAction(char actionType = 'B') : base(actionType){}
}
public abstract class NPCAction : Action<IAgent>
{
    public NPCAction(char actionType = 'N') : base(actionType){}
}
public abstract class SelfAction : Action<NoTarget>
{
    public SelfAction(char actionType = 'S') : base(actionType){}
}
public abstract class EnvironmentAction : Action<NoTarget>
{
    public EnvironmentAction(char actionType = 'E') : base(actionType){}
}

public interface IAction
{
    char actionType {get;} //action type ("N" = NPC, "B" = Building, "E" = Environment, "S" = Self)
    string name {get;}
    string baseDescription {get;}
}

public struct ActionInfoWrapper : IEquatable<ActionInfoWrapper>
{
    public int currentActor;
    public int receiver;
    public float timeToComplete;
    public float energyToComplete;
    public bool? known;
    public float estUtility;
    public float actUtility;
    public float estSuccess;
    public float actSuccess;
    public IAction action;
    public ActionInfoWrapper(int currentActor, int receiver, float timeToComplete, float energyToComplete, bool? known, float estUtility, float actUtility, float estSuccess, float actSuccess, IAction action)
    {
        this.receiver = receiver;
        this.currentActor = currentActor;
        this.timeToComplete = timeToComplete;
        this.energyToComplete = energyToComplete;
        this.known = known;
        this.estUtility = estUtility;
        this.actUtility = actUtility;
        this.estSuccess = estSuccess;
        this.actSuccess = actSuccess;
        this.action = action;
    }
    public bool Equals(ActionInfoWrapper other)
    {
        if(receiver == other.receiver && currentActor == other.currentActor && action.name == other.action.name) return true;
        return false;
    }

    public override bool Equals(object other)
    {
        if(other is ActionInfoWrapper otherInf) return Equals(otherInf);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(receiver, currentActor, action.name);
    }

    public static bool operator == (ActionInfoWrapper a, ActionInfoWrapper b) => a.Equals(b);
    public static bool operator != (ActionInfoWrapper a, ActionInfoWrapper b) => !a.Equals(b);
}