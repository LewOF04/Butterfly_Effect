using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class GiveFood : NPCAction
{
    private const float baseFoodIncrease = 5f;
    public GiveFood() : base('N'){}

    public override char actionType => 'N';
    public override string name => "Give Food";
    public override string baseDescription => "The npc would like to help another by giving them food.";

    protected override float baseTime => 0.5f;
    protected override float baseEnergy => 4;
    protected override float complexity => 5f;
    protected override float baseUtility => 37.5f;

    protected override List<int> utilityPosTraits => new List<int>{1}; 
    protected override List<int> utilityNegTraits => new List<int>{9, 0};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(IAgent performer, IAgent target)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, target);
        if(energyToComplete == -1) getEnergyToComplete(performer, target);
        if(actSuccess == -1) computeSuccess(performer, target);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.food, 0f, 100f, 0.25f, 2.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.morality, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.nutrition, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcExpMultiplier(target.stats.food, 0f, 100f, 2f, 0.25f, 4f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);

        //==================Energy, Time and Success Effectors==================
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(timeToComplete, 0f, 24f, 2f, 0.25f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));

        //==================Relative Benefit Weightings==================
        effectors.Add(ActionMaths.calcMultiplier(baseFoodIncrease, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        
        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(IAgent performer, IAgent target)
    {
        if(estUtility != -1) return estUtility;
        
        float baseEst;
        if(actUtility == -1) baseEst = computeUtility(performer, default);
        else baseEst = actUtility;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        baseEst = ActionMaths.ApplyWeightedMultipliers(baseEst, effectors, weights);

        baseEst = ActionMaths.addTraitWeights(performer, baseEst, utilityPosTraits, true);
        baseEst = ActionMaths.addTraitWeights(performer, baseEst, utilityNegTraits, false); 

        estUtility = ActionMaths.rationalityNoise(baseEst, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        if(!dataController.TryGetAgent(currentActor, out var performer)) return;
        if(!dataController.TryGetAgent(receiver, out var target)) return;

        float percentMulti = percentComplete / 100;
        string description = performer.fullName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours giving food to " + target.fullName + ".";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        //add description and result levels based on success
        float multiplier;
        if(successInfo.success == true) {
            description += "They successfully gave them food ";

            if(successInfo.quality < 0.25f) {description += "but " + target.fullName + " didn’t seem very appreciative."; multiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += "but it wasn’t as helpful as they had hoped."; multiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += "and " + target.fullName + " was grateful for the meal."; multiplier = 1f;}
            else {description += "and " + target.fullName + " was extremely thankful for the generous help."; multiplier = 1.33f;}
        }
        else {
            description += "Their attempt to give food was not a success ";

            if(successInfo.quality < 0.25f) {description += "they couldn’t find " + target.fullName + " to give the food."; multiplier = 0f;}
            else if(successInfo.quality < 0.5f) {description += target.fullName + " refused to accept the food."; multiplier = -0.1f;}
            else if(successInfo.quality < 0.75f) {description += "the food was spoiled or unsuitable."; multiplier = -0.2f;}
            else {description += "and the situation ended awkwardly, leaving both parties uncomfortable."; multiplier = -0.3f;}
        }

        if(percentComplete != 100f) description += " The attempt concluded " + percentComplete.ToString("0.00") + "% of the way through.";
                
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

        //food changes
        float foodChange = Mathf.Max(0f, multiplier) * baseFoodIncrease; 
        performer.stats.food = Mathf.Max(0f, performer.stats.food - foodChange);
        target.stats.food = Mathf.Min(100f, target.stats.food + foodChange);
        description += " and gave "+foodChange.ToString("0.00")+" food to the target.";

        //happiness changes
        float happinessChange = 7f;
        happinessChange *= multiplier;
        performer.stats.happiness = Mathf.Clamp(performer.stats.happiness + happinessChange, 0f, 100f);
        target.stats.happiness = Mathf.Clamp(target.stats.happiness + (happinessChange * 0.5f), 0f, 100f);
        description += " The performers happiness changed by "+happinessChange.ToString("0.00")+" and the targets by "+(happinessChange * 0.5f).ToString("0.00")+".";

        //morality changes
        if (successInfo.success)
        {
            performer.attributes.morality = Mathf.Min(100f, performer.attributes.morality + 3f);
            description += "The performer's morality increased by 3.";
        }

        //relationship changes
        float relationshipChange = 10f;
        relationshipChange *= multiplier;
        description += "The relationship between the two has changed by "+relationshipChange.ToString("0.00");

        //determine positivity
        bool wasPosPerf = false;
        bool wasPosRec = false;
        if(multiplier > 0) {wasPosPerf = true; wasPosRec = true;}
        else {wasPosPerf = false; wasPosRec = false;}

        float severity = 20 * Mathf.Abs(multiplier); //determine severity
        HistoryManager.Instance.AddNPCMemory(name, description, severity, actionTime, performer.id, target.id, wasPosPerf, wasPosRec); //save memory
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(IAgent performer, IAgent target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.75f, 2f)); weights.Add(0.5f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.charisma, 0f, 100f, 0.6f, 2f)); weights.Add(0.4f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.nutrition, 0f, 100f, 2f, 0.75f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.food, 0f, 100f, 2f, 0.75f)); weights.Add(0.8f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);

        actSuccess = weightedSucc;
        return actSuccess;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(IAgent performer, IAgent target)
    {
        if(estSuccess != -1) return estSuccess;
        
        float baseSuccess;
        if(actSuccess == -1) baseSuccess = computeSuccess(performer, default);
        else baseSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(baseSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the IAgent to complete this action
    protected override float getTimeToComplete(IAgent performer, IAgent target)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.charisma, 0f, 100f, 2f, 0.6f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 2f, 0.6f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.6f)); weights.Add(0.2f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.happiness, 0f, 100f, 2f, 0.6f)); weights.Add(0.3f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, effectors, weights);

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.World.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the IAgent to compelete this action
    protected override float getEnergyToComplete(IAgent performer, IAgent target)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.charisma, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2f, 0.25f)); weights.Add(0.2f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.World.chaosModifier);
        return energyToComplete;
    }

    protected override bool isKnown(IAgent performer)
    {
        if(performer.stats.food < 5) return false;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}