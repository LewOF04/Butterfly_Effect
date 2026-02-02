using UnityEngine;
using System.Collections.Generic;

public abstract class Action1 : NPCAction
{
    private readonly float baseTime = 1.0f; 
    private readonly float baseEnergy = 1.0f; 
    public readonly List<int> utilityPosTraits = new List<int>{}; 
    public readonly List<int> utilityNegTraits = new List<int>{};
    public readonly List<int> successPosTraits = new List<int>{}; 
    public readonly List<int> successNegTraits = new List<int>{};
    public abstract double computeUtility(NPC performer, T receiver); //compute the empirical utility of the action 
    public abstract double estimateUtility(NPC performer, T receiver); //compute the performers perceived utility of the action
    public abstract void actionResult(NPC performer, T receiver); //compute the result of the action being performed
    public abstract float computeSuccess(NPC performer, T receiver); //computer the likelihood this action will be a success
    public abstract float estimateSuccess(NPC performer, T receiver); //compute the estimated chance this action will be a succss from the performers perspective
    public abstract bool isKnown(NPC performer, T receiver); //check that this action would be known to the NPC
    public abstract float getTimeToComplete(NPC performer, T receiver); //calculate how much time it would take for the NPC to complete this action
    public abstract float getEnergyToComplete(NPC performer, T receiver); //calculate how much energy it would take the NPC to compelete this action
    public Action1(){} 
}