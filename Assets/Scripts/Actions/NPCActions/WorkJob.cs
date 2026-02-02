using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;

[Preserve]
public class WorkJob : SelfAction
{
    public WorkJob() : base('S'){}
    private readonly float baseTime = 8.0f; 
    private readonly float baseEnergy = 50.0f; 
    public readonly List<int> utilityPosTraits = new List<int>{3}; 
    public readonly List<int> utilityNegTraits = new List<int>{4};
    public readonly List<int> successPosTraits = new List<int>{}; 
    public readonly List<int> successNegTraits = new List<int>{};

    //compute the empirical utility of the action
    public override double computeUtility(NPC performer, byte? _)
    {
        float energyDiff = performer.stats.energy - baseEnergy;
    }

    //compute the performers perceived utility of the action
    public override double estimateUtility(NPC performer, byte? _)
    {
        
    }

    //compute the result of the action being performed
    public override void actionResult(NPC performer, byte? _)
    {
        
    }

    //computer the likelihood this action will be a success
    public override float computeSuccess(NPC performer, byte? _)
    {
        
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    public override float estimateSuccess(NPC performer, byte? _)
    {
        
    }

    //check that this action would be known to the NPC
    public override bool isKnown(NPC performer, byte? _)
    {
        
    }

    //calculate how much time it would take for the NPC to complete this action
    public override float getTimeToComplete(NPC performer, byte? _)
    {
        //dexterity, constitution, energy, nutrition, condition
        float dextWeight = ActionMaths.calcMultiplier(performer.attributes.dexterity, 0.5, 1.5, 1);

        float constWeight = ActionMaths.calcMultiplier(performer.attributes.constitution, 0.5, 1.5, 1);
    }

    //calculate how much energy it would take the NPC to compelete this action
    public override float getEnergyToComplete(NPC performer, byte? _)
    {
        
    }
}