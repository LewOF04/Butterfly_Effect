using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class HaveChild : NPCAction
{
    private const float baseCost = 30f;
    public HaveChild() : base('N'){}

    public override char actionType => 'N';
    public override string name => "Have Child";
    public override string baseDescription => "The agent will have a child with another agent.";

    protected override float baseTime => 3f;
    protected override float baseEnergy => 50f;
    protected override float complexity => 15f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{}; 
    protected override List<int> utilityNegTraits => new List<int>{};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(IAgent performer, IAgent target)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, default);
        if(energyToComplete == -1) getEnergyToComplete(performer, default);
        if(actSuccess == -1) computeSuccess(performer, default);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.food, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.food, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Energy, Time and Success Effectors==================
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(timeToComplete, 0f, 24f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));
        
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

        //weight based on number of people living in the house
        if(dataController.TryGetBuilding(performer.parentBuilding, out var building))
        {
            effectors.Add(ActionMaths.calcMultiplier((float) building.inhabitants.Count, 0f, 20f, 4f, 0.25f)); weights.Add(0.8f);
        }
        else effectors.Add(0.1f); weights.Add(0.9f);

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
        string description = performer.fullName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours trying to have a child with " + target.fullName + ".";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        //add description and result levels based on success
        float multiplier;
        if(successInfo.success == true) {
            description += "They successfully conceived a child ";

            if(successInfo.quality < 0.25f) {description += "but the circumstances were difficult and uncertain."; multiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += "though it took effort and did not go entirely smoothly."; multiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += "and everything proceeded as expected."; multiplier = 1f;}
            else {description += "and the process went exceptionally well, with both parents in good condition."; multiplier = 1.33f;}
        }
        else {
            description += "Their attempt to have a child was not successful ";

            if(successInfo.quality < 0.25f) {description += "they were unable to proceed due to circumstances."; multiplier = 0f;}
            else if(successInfo.quality < 0.5f) {description += "the attempt did not succeed despite their efforts."; multiplier = -0.1f;}
            else if(successInfo.quality < 0.75f) {description += "complications prevented success this time."; multiplier = -0.2f;}
            else {description += "and the experience left them emotionally strained."; multiplier = -0.3f;}
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

        //wealth changes
        performer.stats.wealth = Mathf.Max(0f, performer.stats.wealth - baseCost);
        target.stats.wealth = Mathf.Max(0f, target.stats.wealth - baseCost);
        description += " and cost "+baseCost.ToString("0.00")+" wealth from the receiver and performer.";

        //happiness changes
        if(multiplier > 1f)
        {
            performer.stats.happiness = Mathf.Min(100f, performer.stats.happiness + 20f);
            target.stats.happiness = Mathf.Min(100f, target.stats.happiness + 20f);
            description += " The performer and targets happiness increased by 20.";
        }else if(multiplier < -0.2f)
        {
            performer.stats.happiness = Mathf.Max(0f, performer.stats.happiness - 10f);
            target.stats.happiness = Mathf.Max(0f, target.stats.happiness - 10f);
            description += " The performer and targets happiness decreased by 10.";
        }

        //relationship changes
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        thisRel.value = Mathf.Min(100f, thisRel.value + 30f);
        description += " The relationship between the two improved by 30.";

        //determine positivity
        bool wasPosPerf = false;
        bool wasPosRec = false;
        if(multiplier > 0) {wasPosPerf = true; wasPosRec = true;}
        else {wasPosPerf = false; wasPosRec = false;}

        DataController.Instance.npcManager.generateSingleNPC(performer);
        
        float severity = 30f * Mathf.Abs(multiplier); //determine severity
        HistoryManager.Instance.AddNPCMemory(name, description, severity, actionTime, performer.id, target.id, wasPosPerf, false); //save memory
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(IAgent performer, IAgent target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.wisdom, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);

        //add 5% for each time this IAgent has performed the action in the past
        List<NPCEvent> events = dataController.eventsPerNPCStorage[currentActor];
        foreach(NPCEvent thisEvent in events)
        {
            if(thisEvent.actionName == name && thisEvent.performer == currentActor)
            {
               weightedSucc = Mathf.Min(100f, weightedSucc + weightedSucc * 0.05f); 
            }
        }  

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

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

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

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.World.chaosModifier);
        return energyToComplete;
    }

    protected override bool isKnown(IAgent performer)
    {
        if(performer.stats.wealth < baseCost) return false;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}