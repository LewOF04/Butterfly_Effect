using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class WorkJob : SelfAction
{
    public WorkJob() : base('S'){}

    public override char actionType => 'S';
    public override string name => "Work Job";

    protected override float baseTime => 8f;
    protected override float baseEnergy => 50f;
    protected override float complexity => 30f;

    protected override List<int> utilityPosTraits => new List<int>{3}; 
    protected override List<int> utilityNegTraits => new List<int>{4};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(NPC performer, NoTarget _)
    {
        if(this.actUtility != -1) return this.actUtility;
        float baseUtility = 50.0f;

        float timeToComplete = getTimeToComplete(performer, default);
        float energyToComplete = getEnergyToComplete(performer, default);
        float successChance = computeSuccess(performer, default);

        List<float> effectors = new List<float>();
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0.5f, 1.5f)); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0.5f, 1.5f)); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0.5f, 1.5f)); 

        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 1.5f, 0.5f));

        List<float> weights = new List<float>{0.3f, 0.6f, 0.5f, 0.8f};


        return 0.0f;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NoTarget _)
    {
        return 0.0f;
    }

    //compute the result of the action being performed
    protected override void actionResult(NPC performer, NoTarget _)
    {

    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NoTarget _)
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 1.5f, 0.5f)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 1.5f, 0.5f)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 1.5f, 0.5f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 1.5f, 0.5f)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 1.5f, 0.5f)); //condition multiplier

        return 0.0f;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(NPC performer, NoTarget _)
    {
        return 0.0f;
    }

    //check that this action would be known to the NPC
    protected override bool isKnown(NPC performer, NoTarget _)
    {
        return false;
    }

    //calculate how much time it would take for the NPC to complete this action
    protected override float getTimeToComplete(NPC performer, NoTarget _)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.7f, 0.7f, 0.5f, 0.2f, 0.2f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);

        float result = ActionMaths.addChaosRandomness(weightedBase, dataController.worldManager.chaosModifier);

        timeToComplete = result;
        return result;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NoTarget _)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.3f, 0.3f, 0.8f, 0.7f, 0.5f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, multipliers, weights);

        float result = ActionMaths.addChaosRandomness(weightedBase, dataController.worldManager.chaosModifier);

        energyToComplete = result;
        return result;
    }

    protected override List<float> getTimeAndEnergyMultipliers(NPC performer)
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 1.5f, 0.5f)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 1.5f, 0.5f)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 1.5f, 0.5f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 1.5f, 0.5f)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 1.5f, 0.5f)); //condition multiplier

        return multipliers;
    }
}