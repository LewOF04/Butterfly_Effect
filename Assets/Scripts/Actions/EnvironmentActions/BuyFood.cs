using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class BuyFood : EnvironmentAction
{
    private const float baseFoodIncrease = 20f;
    public BuyFood() : base('E'){}

    public override char actionType => 'E';
    public override string name => "Buy Food";
    public override string baseDescription => "Buy food in order to consume it at a later date or give to others.";

    protected override float baseTime => 1f;
    protected override float baseEnergy => 6f;
    protected override float complexity => 5f;
    protected override float baseUtility => 45f;

    protected override List<int> utilityPosTraits => new List<int>{0}; 
    protected override List<int> utilityNegTraits => new List<int>{};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(IAgent performer, NoTarget _)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, default);
        if(energyToComplete == -1) getEnergyToComplete(performer, default);
        if(actSuccess == -1) computeSuccess(performer, default);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.nutrition, 100f, 0f, 0.25f, 2f, 4f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.constitution));
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.food, 100f, 0f, 0.25f, 4f, 3f)); weights.Add(1.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Energy, Time and Success Effectors==================
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.calcMultiplier(timeToComplete, 0f, 24f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));

        //==================Relative Benefit Weightings==================
        effectors.Add(ActionMaths.calcMultiplier(baseFoodIncrease, 0f, 100f, 0.25f, 2f)); weights.Add(1f);
        
        actUtility = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(IAgent performer, NoTarget _)
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

    protected override void innerPerformAction(float percentComplete)
    {
        if(!dataController.TryGetAgent(currentActor, out var performer)) return;
        float baseCost = dataController.World.costPerFood * baseFoodIncrease;

        float percentMulti = percentComplete / 100;
        string description = performer.fullName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours shopping for food.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        //add description and result levels based on success
        float costMultiplier;
        float volumeMultiplier;
        if(successInfo.success == true) {
            description += " They successfully purchased food ";
            
            if(successInfo.quality < 0.25f) {description += "but once they bought them they found out there was mold!"; costMultiplier = 1.1f; volumeMultiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += "but they couldn't find all of the items they wanted."; costMultiplier = 1f; volumeMultiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += "and they purchased everything they wanted."; costMultiplier = 1f; volumeMultiplier = 1f;}
            else {description += "and it was even on a discount."; costMultiplier = 0.8f; volumeMultiplier = 1.33f;}
        }
        else {
            description += "They weren't able to buy food.";
            volumeMultiplier = 0f;
            costMultiplier = 0f;
        }

        if(percentComplete != 100f) description += " They had to stop buying "+percentComplete.ToString()+"% of the way through.";
        
        float actionTime = dataController.World.gameTime;

        //perform changes to stats/attributes
        description += "\n";

        //energy changes
        float energyMinus = energyToComplete * percentMulti;
        performer.stats.energy = Mathf.Max(0f, performer.stats.energy - energyMinus);
        description += "They spent "+energyMinus.ToString("0.00")+" energy, ";

        //time changes
        float timeMinus = timeToComplete * percentMulti;
        description += timeMinus.ToString("0.00")+" hours";

        //wealth changes
        float wealthCost = baseCost * costMultiplier; 
        performer.stats.wealth = Mathf.Max(0f, performer.stats.wealth - wealthCost);
        description += " and "+wealthCost.ToString("0.00")+" wealth.";

        //happiness changes
        if(performer.traits.Contains(0)) 
        {
            float happinessGain = Mathf.Min(100f, performer.stats.happiness + performer.stats.happiness * 0.1f);
            description += " They gained "+happinessGain.ToString("0.00")+" since they're a shopaholic.";
        }

        //food changes
        float foodGain = baseFoodIncrease * volumeMultiplier;
        performer.stats.food = Mathf.Min(100f, performer.stats.food + foodGain);
        description += " In total they walked away with "+foodGain.ToString("0.00")+" food.";

        //determine positivity
        bool wasPosPerf = successInfo.success;

        float severity = 5f; //determine severity
        HistoryManager.Instance.AddNPCMemory(name, description, severity, actionTime, performer.id, -1, wasPosPerf, false); //save memory
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(IAgent performer, NoTarget _)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.charisma, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);

        actSuccess = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);
        return actSuccess;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(IAgent performer, NoTarget _)
    {
        if(estSuccess != -1) return estSuccess;
        
        float baseSuccess;
        if(actSuccess == -1) baseSuccess = computeSuccess(performer, default);
        else baseSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(baseSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the IAgent to complete this action
    protected override float getTimeToComplete(IAgent performer, NoTarget _)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.intelligence, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.charisma, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, effectors, weights);

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.World.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the IAgent to compelete this action
    protected override float getEnergyToComplete(IAgent performer, NoTarget _)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.intelligence, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.rationality, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.World.chaosModifier);
        return energyToComplete;
    }

    protected override bool isKnown(IAgent performer)
    {
        float baseCost = dataController.World.costPerFood * baseFoodIncrease;
        //action isn't possible because of a lack of money
        if (baseCost > performer.stats.wealth) return false;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;
        
        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}