using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;

DataController dataController = DataController.Instance;

[Preserve]
public class WorkJob : SelfAction
{
    public WorkJob() : base('S'){}

    private override readonly float baseTime = 8.0f; 
    private override readonly float baseEnergy = 50.0f; 
    private override readonly float complexity = 30.0f;

    private override readonly List<int> utilityPosTraits = new List<int>{3}; 
    private override readonly List<int> utilityNegTraits = new List<int>{4};
    private override readonly List<int> successPosTraits = new List<int>{}; 
    private override readonly List<int> successNegTraits = new List<int>{};

    //compute the empirical utility of the action
    public override double computeUtility(NPC performer, byte? _)
    {
        if(this.actUtility != -1) return this.actUtility;
        float baseUtility = 50.0f;

        float timeToComplete = getTimeToComplete(performer, null);
        float energyToComplete = getEnergyToComplete(performer, null);
        float successChance = computeSuccess(performer, null);

        List<float> effectors = new List<float>();
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0.5, 1.5)); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0.5, 1.5)); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0.5, 1.5)); 

        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 1.5, 0.5));

        List<float> weights = new List<float>{0.3, 0.6, 0.5, 0.8};



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
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 1.5, 0.5)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 1.5, 0.5)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 1.5, 0.5)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 1.5, 0.5)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 1.5, 0.5)); //condition multiplier
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
        if(timeToComplete != -1) return timeToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers();
        List<float> weights = new List<float>{0.7, 0.7, 0.5, 0.2, 0.2};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);

        float result = ActionMaths.addChaosRandomness(weightedBase, dataController.worldManager.chaosModifier);

        timeToComplete = result;
        return result;
    }

    //calculate how much energy it would take the NPC to compelete this action
    public override float getEnergyToComplete(NPC performer, byte? _)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers();
        List<float> weights = new List<float>{0.3, 0.3, 0.8, 0.7, 0.5};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, multipliers, weights);

        float result = ActionMaths.addChaosRandomness(weightedBase, dataController.worldManager.chaosModifier);

        energyToComplete = result;
        return result;
    }

    public override List<float> getTimeAndEnergyMultipliers()
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 1.5, 0.5)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 1.5, 0.5)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 1.5, 0.5)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 1.5, 0.5)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 1.5, 0.5)); //condition multiplier

        return multipliers;
    }
}