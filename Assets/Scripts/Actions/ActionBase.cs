using UnityEngine;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

public abstract class ActionBase<T> : IActionBase
{
    protected ActionBase() { }
    protected ActionBase(char _actionType) { }

    protected DataController dataController => DataController.Instance;

    public abstract char actionType { get; }
    public virtual string name => GetType().Name;

    //stores for quick reference is talking about the same npc
    protected int currentActor = -1;
    protected int? receiver = null;
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
    protected abstract List<int> utilityPosTraits {get;} //the traits that will have an positive impact on perceived utility
    protected abstract List<int> utilityNegTraits {get;} //the traits that will have an negative impact on perceived utility
    protected abstract List<int> successPosTraits {get;}//the traits that will have a positive impact on the action success
    protected abstract List<int> successNegTraits {get;} //the traits that will have a negative impact on the action success
    protected abstract float computeUtility(NPC performer, T receiver); //compute the empirical utility of the action 
    protected abstract float estimateUtility(NPC performer, T receiver); //compute the performers perceived utility of the action
    protected abstract void actionResult(NPC performer, T receiver); //compute the result of the action being performed
    protected abstract float computeSuccess(NPC performer, T receiver); //computer the likelihood this action will be a success
    protected abstract float estimateSuccess(NPC performer, T receiver); //compute the estimated chance this action will be a succss from the performers perspective
    protected abstract float getTimeToComplete(NPC performer, T receiver); //calculate how much time it would take for the NPC to complete this action
    protected abstract float getEnergyToComplete(NPC performer, T receiver); //calculate how much energy it would take the NPC to compelete this action
    protected abstract List<float> getTimeAndEnergyMultipliers(NPC performer); //get the list of multipliers that effect this actions time and energy 
    
    //check that this action would be known to the NPC
    protected bool isKnown(NPC performer)
    {
        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        List<BuildingEvent> buildingEvents = dataController.buildingEventsPerNPCStorage[performer.id];
        foreach(BuildingEvent thisEvent in buildingEvents) if(thisEvent.actionName == name) return true;

        return false;
    } 

    public ActionInfoWrapper computeAction(NPC performer, T receiver)
    {
        this.resetAction();
        this.currentActor = performer.id;

        this.receiver = receiver switch
        {
            NPC npc         => npc.id,
            Building build  => build.id,
            _               => null
        };


        energyToComplete = getEnergyToComplete(performer, receiver);
        timeToComplete = getTimeToComplete(performer, receiver);
        known = isKnown(performer);
        actUtility = computeUtility(performer, receiver);
        estUtility = estimateUtility(performer, receiver);
        actSuccess = computeSuccess(performer, receiver);
        estSuccess = estimateSuccess(performer, receiver);

        return new ActionInfoWrapper(performer.id, this.receiver, timeToComplete, energyToComplete, known, estUtility, actUtility, estSuccess, actSuccess);
    }

    /*
    Reset the action information to create new action instance
    */
    public void resetAction()
    {
        currentActor = -1;
        receiver = null;
        timeToComplete = -1f;
        energyToComplete = -1f;
        known = null;
        estUtility = -1f;
        actUtility = -1f;
        estSuccess = -1f;
        actSuccess = -1f;
    }
}

public abstract class BuildingAction : ActionBase<Building>
{
    public BuildingAction(char actionType = 'B') : base(actionType){}
}
public abstract class NPCAction : ActionBase<NPC>
{
    public NPCAction(char actionType = 'N') : base(actionType){}
}
public abstract class SelfAction : ActionBase<NoTarget>
{
    public SelfAction(char actionType = 'S') : base(actionType){}
}
public abstract class EnvironmentAction : ActionBase<NoTarget>
{
    public EnvironmentAction(char actionType = 'E') : base(actionType){}
}

public interface IActionBase
{
    char actionType {get;} //action type ("N" = NPC, "B" = Building, "E" = Environment, "S" = Self)
    string name {get;}
}

public struct ActionInfoWrapper
{
    public int currentActor;
    public int? receiver;
    public float timeToComplete;
    public float energyToComplete;
    public bool? known;
    public float estUtility;
    public float actUtility;
    public float estSuccess;
    public float actSuccess;
    public ActionInfoWrapper(int currentActor, int? receiver, float timeToComplete, float energyToComplete, bool? known, float estUtility, float actUtility, float estSuccess, float actSuccess)
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
    }
}