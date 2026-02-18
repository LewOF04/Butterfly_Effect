using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class WorkJob : SelfAction
{
    private const float baseEarning = 5f;
    public WorkJob() : base('S'){}

    public override char actionType => 'S';
    public override string name => "Work Job";
    public override string baseDescription => "The NPC if they have a job, can go to work to earn some money at the cost of energy and time.";

    protected override float baseTime => 8f;
    protected override float baseEnergy => 50f;
    protected override float complexity => 5f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{3}; 
    protected override List<int> utilityNegTraits => new List<int>{4};
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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(1 - Mathf.InverseLerp(0f, 100f, performer.attributes.fortitude));
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.wealth, 0f, 100f, 5f, 0.25f, 5f)); weights.Add(0.8f);

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        //multiplier to make result benefits relative
        effectors.Add(ActionMaths.calcMultiplier(baseEarning, 0f, 100f, 1f, 1.5f)); weights.Add(0.8f); 

        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NoTarget _)
    {
        if(estUtility != -1) return estUtility;
        
        if(actUtility == -1) actUtility = computeUtility(performer, default);

        estUtility = ActionMaths.addTraitWeights(performer, actUtility, utilityPosTraits, true);
        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityNegTraits, false); 

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        float percentMulti = percentComplete/100;
        string description = performer.npcName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours at work.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        float wealthMultiplier;
        if(successInfo.success == true) {
            description += "They successfully completed their work ";
            
            if(successInfo.quality < 0.25f) {description += "but it was a terrible work performance."; wealthMultiplier = 0.666f;}
            else if(successInfo.quality < 0.5f) {description += "but they didn't do a great job."; wealthMultiplier = 0.832f;}
            else if(successInfo.quality < 0.75f) {description += "and they did a good job."; wealthMultiplier = 1f;}
            else {description += "and they did some amazing work."; wealthMultiplier = 1.166f;}
        }
        else {
            description += "Their work was not a success ";
            if(successInfo.quality < 0.25f) {description += "but they'll finish off on their next shift."; wealthMultiplier = 0.5f;}
            else if(successInfo.quality < 0.5f) {description += "but they made a good effort."; wealthMultiplier = 0.332f;}
            else if(successInfo.quality < 0.75f) {description += "and they made no effort to correct it."; wealthMultiplier = 0.166f;}
            else {description += "and they caused a lot of problems."; wealthMultiplier = 0f;}
        }

        if(percentComplete != 100f) description += " They had to stop working after doing "+percentComplete.ToString()+"% completion.";
        
        float actionTime = dataController.worldManager.gameTime + (24f - performer.timeLeft);

        description += "\n";

        float energyMinus = energyToComplete * percentMulti;
        performer.stats.energy -= energyMinus;
        description += "They spent "+energyMinus.ToString("0.00")+" energy, ";

        float timeMinus = timeToComplete * percentMulti;
        performer.timeLeft -= timeMinus;
        description += timeMinus.ToString("0.00")+" hours ";

        float wealthGain = baseEarning * wealthMultiplier;
        performer.stats.wealth = Mathf.Min(100f, performer.stats.wealth + wealthGain);
        description += "and increased their wealth by "+wealthGain.ToString("0.00");

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
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f); //condition multiplier

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

    protected List<float> getTimeAndEnergyMultipliers(NPC performer)
    {
        List<float> multipliers = new List<float>{};

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2f, 0.25f)); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 2f, 0.25f)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); //condition multiplier

        return multipliers;
    }

    protected override bool isKnown(NPC performer)
    {
        if(performer.hasJob == false) return false;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}