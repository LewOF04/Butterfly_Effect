using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class EatFood : SelfAction
{
    private const float baseConsumption = 5f;
    private const float baseNutritionGain = 20f;
    public EatFood() : base('S'){}

    public override char actionType => 'S';
    public override string name => "Eat Food";
    public override string baseDescription => "The NPC if they have food, can eat it to restore nutrition levelss.";

    protected override float baseTime => 0.5f;
    protected override float baseEnergy => 3f;
    protected override float complexity => 0f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{9}; 
    protected override List<int> utilityNegTraits => new List<int>{};
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

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.nutrition, 100f, 0f, 0.25f, 4f, 4f)); weights.Add(1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.food, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);

        //==================Energy, Time and Success Effectors==================
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.food - baseConsumption, 0f, 100f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));

        //==================Relative Benefit Weightings==================
        effectors.Add(ActionMaths.calcMultiplier(baseNutritionGain, 0f, 100f, 0.25f, 2f)); weights.Add(1f);
        
        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
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

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];

        float percentMulti = percentComplete / 100;
        string description = performer.npcName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " eating food.";

        //the amount of food that needs to be consumed is relative to the condition
        float foodConsumed = baseConsumption * Mathf.Lerp(0.75f, 1.5f, Mathf.InverseLerp(0f, 100f, performer.stats.condition));
        float foodPerc = performer.stats.food / foodConsumed; 

        //the level of completion is the lower of if we have to stop for lack of food or previous precent stoppage
        percentMulti = Mathf.Min(percentMulti, foodPerc);

        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentMulti * 100);

        //add description and result levels based on success
        float foodMultiplier;
        if(successInfo.success == true) {
            description += "They successfully ate food ";
            
            if(successInfo.quality < 0.25f) {description += "but they found themselves feeling a bit sick whilst eating."; foodMultiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += "but didn't eat as much as they would've liked."; foodMultiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += "and they ate every single bite they could."; foodMultiplier = 1f;}
            else {description += "and it reminded them of their mother's cooking."; foodMultiplier = 1.33f;}
        }
        else {
            description += "They didn't successfully eat food ";
            if(successInfo.quality < 0.25f) {description += "they dropped all of their clean cutlery on the floor."; foodMultiplier = 0.2f;}
            else if(successInfo.quality < 0.5f) {description += "they only had a spoon, which made it quite hard to eat."; foodMultiplier = 0.4f;}
            else if(successInfo.quality < 0.75f) {description += "they found the food too spicy to eat."; foodMultiplier = 0.6f;}
            else {description += "they found a slug on their clean lettuce!"; foodMultiplier = 0.8f;}
        }

        if(percentComplete != 100f) description += " Their meal finished after "+(percentMulti*100).ToString("0.00")+"% of the way through.";
        
        float actionTime = dataController.worldManager.gameTime + (24f - performer.timeLeft);

        //perform changes to stats/attributes
        description += "\n";

        //energy changes
        float energyMinus = energyToComplete * percentMulti;
        performer.stats.energy -= energyMinus;
        description += "They spent "+energyMinus.ToString("0.00")+" energy, ";

        //time changes
        float timeMinus = timeToComplete * percentMulti;
        performer.timeLeft -= timeMinus;
        description += timeMinus.ToString("0.00")+" hours";

        //food changes
        float foodMinus = baseConsumption * percentMulti * foodMultiplier; 
        performer.stats.food = Mathf.Max(0f, performer.stats.food - foodMinus);
        description += " and consumed "+foodMinus.ToString("0.00")+" food.";

        //nutrition changes
        float nutritionChange;
        if(successInfo.success) nutritionChange = baseNutritionGain * percentMulti;
        else  nutritionChange = 0;

        performer.stats.nutrition = Mathf.Min(100f, performer.stats.nutrition + nutritionChange);
        description += " The food improved their nutrition by "+nutritionChange.ToString("0.00")+".";

        //happiness changes
        if (performer.traits.Contains(9))
        {
            performer.stats.happiness = Mathf.Min(100f, performer.stats.happiness + performer.stats.happiness * 0.1f);
        }

        //determine positivity
        bool wasPosPerf = successInfo.success;

        float severity = 3f;
        dataController.historyManager.AddNPCMemory(name, description, severity, actionTime, performer.id, -1, wasPosPerf, false); //save memory
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NoTarget _)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.food, 100f, 0f, 2f, 0.25f, 4f)); weights.Add(1f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);

        //add 5% for each time this NPC has performed the action in the past
        List<NPCEvent> events = dataController.eventsPerNPCStorage[currentActor];
        foreach(NPCEvent thisEvent in events)
        {
            if(thisEvent.actionName == name && thisEvent.performer == currentActor)
            {
               weightedSucc = Mathf.Min(100f, weightedSucc * 0.05f); 
            }
        }  

        actSuccess = weightedSucc;
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

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 2f, 0.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.food, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, effectors, weights);

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NoTarget _)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 2f, 0.25f)); weights.Add(0.2f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return energyToComplete;
    }

    protected override bool isKnown(NPC performer)
    {
        if(baseConsumption > performer.stats.food) return false; //false if they don't have enough food to eat

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}