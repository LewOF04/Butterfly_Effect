using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class Sleep : SelfAction
{
    private float baseEnergyAdd = 20f;
    private float baseConditionAdd = 10f;
    private float baseHappinessGain = 10f;
    public Sleep() : base('S'){}

    public override char actionType => 'S';
    public override string name => "Sleep";
    public override string baseDescription => "Sometimes the best course of action is to sleep it off.";

    protected override float baseTime => 10f;
    protected override float baseEnergy => 0f;
    protected override float complexity => 0f;
    protected override float baseUtility => 20f;

    protected override List<int> utilityPosTraits => new List<int>{4}; 
    protected override List<int> utilityNegTraits => new List<int>{3};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(NPC performer, NoTarget _)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, default);
        if(energyToComplete == -1) getEnergyToComplete(performer, default);
        if(actSuccess == -1) computeSuccess(performer, default);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //npc stat/attribute effectors
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f, 5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f, 5f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.2f);

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        //multiplier to make result benefits relative
        effectors.Add(ActionMaths.calcMultiplier(baseEnergyAdd, 0f, 100f, 1f, 1.5f)); weights.Add(1f);
        effectors.Add(ActionMaths.calcMultiplier(baseConditionAdd, 0f, 100f, 1f, 1.5f)); weights.Add(0.8f);

        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NoTarget _)
    {
        if(estUtility != -1) return estUtility;
        
        if(actUtility == -1) actUtility = computeUtility(performer, default);

        List<float> effectors = new List<float>(); List<float> weights = new List<float>();
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);
        estUtility = ActionMaths.ApplyWeightedMultipliers(actUtility, effectors, weights);

        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityPosTraits, true);
        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityNegTraits, false); 

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        float percentMulti = percentComplete/100;
        string description = performer.npcName + " spent " + (timeToComplete*percentMulti).ToString("0.00") + " hours sleeping.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        float sleepMultiplier;
        if(successInfo.success == true) {
            description += "They successfully completed their sleep ";
            
            if(successInfo.quality < 0.25f) {description += "but they had a nightmare about baby pigoens."; sleepMultiplier = 0.666f;}
            else if(successInfo.quality < 0.5f) {description += "but they were tossing and turning throughout the night."; sleepMultiplier = 0.832f;}
            else if(successInfo.quality < 0.75f) {description += "and they felt good waking up."; sleepMultiplier = 1f;}
            else {description += "and they felt like a new person when they woke up."; sleepMultiplier = 1.166f;}
        }
        else {
            description += "Their sleep was not a success ";
            if(successInfo.quality < 0.25f) {description += "but they did manage to get a few winks."; sleepMultiplier = 0.5f;}
            else if(successInfo.quality < 0.5f) {description += "they tried counting sheep, but there were only cows."; sleepMultiplier = 0.332f;}
            else if(successInfo.quality < 0.75f) {description += "and they had a nightmare about their neighbour killing them."; sleepMultiplier = 0.166f;}
            else {description += "and they memorised every part of their ceiling."; sleepMultiplier = 0f;}
        }

        if(percentComplete != 100f) description += " They were woken up "+percentComplete.ToString()+"% through their sleep.";
        
        float actionTime = dataController.worldManager.gameTime + (24f - performer.timeLeft);

        description += "\n";

        float energyGain = baseEnergyAdd * sleepMultiplier;
        performer.stats.energy += energyGain;
        description += "They gained "+energyGain.ToString("0.00")+" energy, ";

        float happinessGain = baseHappinessGain * sleepMultiplier;
        performer.stats.happiness += happinessGain;
        description += happinessGain.ToString()+" happiness, ";

        float timeMinus = timeToComplete * percentMulti;
        performer.timeLeft -= timeMinus;
        description += "with "+timeMinus.ToString("0.00")+" hours of sleep ";

        float conditionGain = baseConditionAdd * sleepMultiplier;
        performer.stats.condition += conditionGain;
        description += "and increased their condition by "+conditionGain.ToString("0.00")+".";

        float severity = 1f;
        int receiver = -1;
        bool wasPositive = true;
        dataController.historyManager.AddNPCMemory(name, description, severity, actionTime, performer.id, receiver, wasPositive, false);
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NoTarget _)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> multipliers = new List<float>();
        List<float> weights = new List<float>();

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f); 

        actSuccess = ActionMaths.ApplyWeightedMultipliers(50f, multipliers, weights);
        return actSuccess;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(NPC performer, NoTarget _)
    {
        if(estSuccess != -1) return estSuccess;
        
        if(actSuccess == -1) estSuccess = computeSuccess(performer, default);
        else estSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(estSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the NPC to complete this action
    protected override float getTimeToComplete(NPC performer, NoTarget _)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.7f, 0.2f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NoTarget _)
    {
        return baseEnergy;
    }

    protected override List<float> getTimeAndEnergyMultipliers(NPC performer)
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); //condition multiplier

        return multipliers;
    }

    protected override bool isKnown(NPC performer)
    {
        return true;
    } 
}