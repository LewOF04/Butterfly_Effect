using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class TrainStrength : SelfAction
{
    private const float attributeGain = 10f;
    public TrainStrength() : base('S'){}

    public override char actionType => 'S';
    public override string name => "Train Strength";
    public override string baseDescription => "You can always work on yourself, this NPC would like to improve their strength.";

    protected override float baseTime => 4f;
    protected override float baseEnergy => 20f;
    protected override float complexity => 10f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{6}; 
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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Energy, Time and Success Effectors==================
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));

        //==================Relative Benefit Weightings==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(1f);
        
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
        string description = performer.npcName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours improving their strength.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        float improvementMultiplier;
        if(successInfo.success == true) {
            description += "They successfully honed their strength";
            if(successInfo.quality < 0.25f) {description += " though the weight nearly proved too much to handle."; improvementMultiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += " their muscles strained, but they managed to push through."; improvementMultiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += " they lifted with solid form and growing power."; improvementMultiplier = 1f;}
            else {description += " raw power surged through them, their might undeniable."; improvementMultiplier = 1.33f;}
        }
        else {
            description += "Their strength training was not a success";
            if(successInfo.quality < 0.25f) {description += ", the effort left them completely overwhelmed."; improvementMultiplier = 0f;}
            else if(successInfo.quality < 0.5f) {description += ", the weight refused to budge no matter their effort."; improvementMultiplier = 0f;}
            else if(successInfo.quality < 0.75f) {description += ", fatigue set in faster than expected."; improvementMultiplier = -0.05f;}
            else {description += ", their confidence took a heavier blow than their muscles did."; improvementMultiplier = -0.1f;}
        }

        if(percentComplete != 100f) description += " The strength training concluded "+percentComplete.ToString("0.00")+"% of the way through.";
        
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

        bool wasPosPerf;
        float attributeChange;

        //attribute changes
        if(improvementMultiplier > 0)
        {
            wasPosPerf = true;
            attributeChange = attributeGain * percentMulti * improvementMultiplier;
            description += " and they gained "+Mathf.Abs(attributeChange).ToString("0.00")+" strength.";
        }
        else
        {
            wasPosPerf = false;
            attributeChange = attributeGain * percentMulti * improvementMultiplier;
            description += " and they lost "+Mathf.Abs(attributeChange).ToString("0.00")+" strength.";
        }
        performer.attributes.charisma = Mathf.Clamp(performer.attributes.strength + attributeChange, 0f, 100f);

        //constitution changes
        float constitutionChange = improvementMultiplier * 3f;
        performer.attributes.constitution = Mathf.Clamp(performer.attributes.constitution + constitutionChange, 0f, 100f);
        description += " Their constitution changed by ";
        if(improvementMultiplier < 0 ) description += "-";
        else if(improvementMultiplier > 0) description += "+";
        description += constitutionChange.ToString("0.00")+".";

        //happiness changes
        if(performer.traits.Contains(6)) {
            float happinessGain = performer.stats.happiness * 0.1f;
            performer.stats.happiness = Mathf.Clamp(performer.stats.happiness + happinessGain, 0, 100f);
            description += " They gained "+happinessGain.ToString("0.00")+" happiness from performing the action.";
        }

        float severity = 3f * Mathf.Abs(improvementMultiplier); //determine severity
        dataController.historyManager.AddNPCMemory(name, description, severity, actionTime, performer.id, -1, wasPosPerf, false); //save memory
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NoTarget _)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.5f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 100f, 0f, 2f, 0.1f, 4f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.75f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.75f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.5f, 2f)); weights.Add(0.1f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 0.75f, 2f)); weights.Add(0.2f);

        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, effectors, weights);

        //add 5% for each time this NPC has performed the action in the past
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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);

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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 2f, 0.25f)); weights.Add(0.4f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 2, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);

        energyToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return energyToComplete;
    }

    protected override bool isKnown(NPC performer)
    {
        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}