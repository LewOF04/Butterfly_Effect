using UnityEngine;
using System.Collections.Generic;

public abstract class ActionBase<T> : IActionBase
{
    /*
    Accurately determine the exact utility of the 
    */
    public char ActionType { get; }
    protected ActionBase(char actionType) => ActionType = actionType;
    public virtual string name => GetType().FullName;
    [Range(0,24)] private readonly float baseTime; //the time it would take for a completely "normal" npc to complete
    [Range(0,100)] private readonly float baseEnergy; //the energy it would take for a completely "normal" npc to complete
    [Range(0,100)] private readonly float complexity; //how complex this action is to complete
    public readonly List<int> utilityPosTraits; //the traits that will have an positive impact on perceived utility
    public readonly List<int> utilityNegTraits; //the traits that will have an negative impact on perceived utility
    public readonly List<int> successPosTraits; //the traits that will have a positive impact on the action success
    public readonly List<int> successNegTraits; //the traits that will have a negative impact on the action success
    public abstract double computeUtility(NPC performer, T receiver); //compute the empirical utility of the action 
    public abstract double estimateUtility(NPC performer, T receiver); //compute the performers perceived utility of the action
    public abstract void actionResult(NPC performer, T receiver); //compute the result of the action being performed
    public abstract float computeSuccess(NPC performer, T receiver); //computer the likelihood this action will be a success
    public abstract float estimateSuccess(NPC performer, T receiver); //compute the estimated chance this action will be a succss from the performers perspective
    public abstract bool isKnown(NPC performer, T receiver); //check that this action would be known to the NPC
    public abstract float getTimeToComplete(NPC performer, T receiver); //calculate how much time it would take for the NPC to complete this action
    public abstract float getEnergyToComplete(NPC performer, T receiver); //calculate how much energy it would take the NPC to compelete this action
    public readonly char actionType;
}

public abstract class BuildingAction : ActionBase<Building>
{
    public BuildingAction(char actionType = 'B') : base(actionType){}
}
public abstract class NPCAction : ActionBase<NPC>
{
    public NPCAction(char actionType = 'N') : base(actionType){}
}
public abstract class SelfAction : ActionBase<byte?>
{
    public SelfAction(char actionType = 'S') : base(actionType){}
}
public abstract class EnvironmentAction : ActionBase<byte?>
{
    public EnvironmentAction(char actionType = 'E') : base(actionType){}
}

public interface IActionBase
{
    char ActionType {get;} //action type ("N" = NPC, "B" = Building, "E" = Environment, "S" = Self)
    string name {get;}
}