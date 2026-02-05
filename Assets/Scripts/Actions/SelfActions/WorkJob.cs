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
        if(actUtility != -1) return actUtility;
        float baseUtility = 50.0f;

        float timeToComplete = getTimeToComplete(performer, default);
        float energyToComplete = getEnergyToComplete(performer, default);
        float successChance = computeSuccess(performer, default);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //npc stat/attribute effectors
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 2f, 0.25f)); weights.Add(0.8f);

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.scarcityMultiplier(dataController.worldManager.gameTime % 24f - timeToComplete, 0f, 24f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(successChance, 0.25f, 2f)); weights.Add(0.2f);

        actUtility = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NoTarget _)
    {
        if(estUtility != -1) return estUtility;
        
        float baseUtility;
        if(actUtility == -1) baseUtility = computeUtility(performer, default);
        else baseUtility = actUtility;

        baseUtility = ActionMaths.addTraitWeights(performer, baseUtility, utilityPosTraits, true);
        baseUtility = ActionMaths.addTraitWeights(performer, baseUtility, utilityNegTraits, false); 

        estUtility = ActionMaths.rationalityNoise(baseUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void actionResult(NPC performer, NoTarget _)
    {

    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NoTarget _)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> multipliers = new List<float>();
        List<float> weights = new List<float>();

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0.25f, 2f)); weights.Add(0.3f); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0.25f, 2f)); weights.Add(0.3f); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0.25f, 2f)); weights.Add(0.8f); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0.25f, 2f)); weights.Add(0.2f); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0.25f, 2f)); weights.Add(0.6f); //condition multiplier

        actSuccess = ActionMaths.ApplyWeightedMultipliers(50f, multipliers, weights);
        return actSuccess;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(NPC performer, NoTarget _)
    {
        if(estSuccess != -1) return estSuccess;
        
        float baseSuccess;
        if(actSuccess == -1) baseSuccess = computeSuccess(performer, default);
        else baseSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(baseSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the NPC to complete this action
    protected override float getTimeToComplete(NPC performer, NoTarget _)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.7f, 0.7f, 0.5f, 0.2f, 0.2f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NoTarget _)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.3f, 0.3f, 0.8f, 0.7f, 0.5f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, multipliers, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return energyToComplete;
    }

    protected override List<float> getTimeAndEnergyMultipliers(NPC performer)
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 2f, 0.25f)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 2f, 0.25f)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 2f, 0.25f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 2f, 0.25f)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 2f, 0.25f)); //condition multiplier

        return multipliers;
    }
}