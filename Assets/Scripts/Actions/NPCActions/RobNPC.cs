using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class RobNPC : NPCAction
{
    private float baseTakePerc = 40f;
    public RobNPC() : base('N'){}

    public override char actionType => 'N';
    public override string name => "Rob Person";
    public override string baseDescription => "It is sometimes easier and quicker to rob someone for their wealth rather than earn it yourself.";

    protected override float baseTime => 10f;
    protected override float baseEnergy => 60f;
    protected override float complexity => 15f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{2}; 
    protected override List<int> utilityNegTraits => new List<int>{1};
    protected override List<int> successPosTraits => new List<int>{}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(NPC performer, NPC target)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, target);
        if(energyToComplete == -1) getEnergyToComplete(performer, target);
        if(actSuccess == -1) computeSuccess(performer, target);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //npc stat/attribute effectors
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.8f);

        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.wealth, 100f, 0f, 0.25f, 2f, 5f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.morality, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);

        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.attributes.morality, 100f, 0f, 2f, 0.25f, 5f)); weights.Add(0.9f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(1.5f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom));

        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NPC target)
    {
        if(estUtility != -1) return estUtility;
        
        if(actUtility == -1) actUtility = computeUtility(performer, default);

        estUtility = ActionMaths.addTraitWeights(performer, actUtility, utilityPosTraits, true);
        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityNegTraits, false); 

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 2f, 0.25f)); weights.Add(Mathf.Clamp(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom), 0.2f, 0.8f));
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.perception));
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(1 - Mathf.InverseLerp(0f, 100f, performer.attributes.fortitude));

        estUtility = ActionMaths.ApplyWeightedMultipliers(estUtility, effectors, weights);    

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        NPC target = dataController.NPCStorage[receiver];

        float percentMulti = percentComplete / 100;
        string description = performer.npcName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours robbing "+target.npcName+".";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        //add description and result levels based on success
        float robMultiplier;
        if(successInfo.success == true) {
            description += "They successfully robbed them ";
            
            if(successInfo.quality < 0.25f) {description += "but "+target.npcName+" saw them coming and defended themselves well."; robMultiplier = 0.33f;}
            else if(successInfo.quality < 0.5f) {description += "but they weren't able to grab everything they wanted."; robMultiplier = 0.66f;}
            else if(successInfo.quality < 0.75f) {description += "and they took everything they could find from "+target.npcName+"."; robMultiplier = 1f;}
            else {description += "and they even found cash hidden in the victims shoes."; robMultiplier = 1.33f;}
        }
        else {
            description += "Their attempt rob was not a success ";
            if(successInfo.quality < 0.25f) {description += "they got spotted too soon and couldn't make an attempt to rob."; robMultiplier = 0f;}
            else if(successInfo.quality < 0.5f) {description += "they got spotted and the victim was able to slap them."; robMultiplier = -0.1f;}
            else if(successInfo.quality < 0.75f) {description += "they were way too loud and got punched squarely in the face for their troubles."; robMultiplier = -0.2f;}
            else {description += "and they'll be aching from bruises and cuts for a while..."; robMultiplier = -0.3f;}
        }

        if(percentComplete != 100f) description += " The robbery attempt concluded "+percentComplete.ToString()+"% of the way through.";
        
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

        //wealth changes
        float wealthTake = Mathf.Max(0f, robMultiplier) * (baseTakePerc/100f) * target.stats.wealth; 
        performer.stats.wealth += wealthTake;
        target.stats.wealth -= wealthTake;
        description += " and took "+wealthTake.ToString("0.00")+" wealth from the target.";

        //health changes
        float healthDiff = Mathf.Abs(robMultiplier) * 15f;
        if(robMultiplier < 0)
        {
            float condChange = Mathf.Pow(healthDiff, 1 - Mathf.InverseLerp(0f, 100f, performer.attributes.constitution));
            performer.stats.condition = Mathf.Max(0f, performer.stats.condition - condChange);
            description += " The performer was damaged by "+condChange.ToString("0.00")+" health.";
        } else if (robMultiplier > 0)
        {
            float condChange = Mathf.Pow(healthDiff, 1 - Mathf.InverseLerp(0f, 100f, target.attributes.constitution));
            target.stats.condition = Mathf.Max(0f, target.stats.condition - condChange);
            description += " The target was damaged by "+condChange.ToString("0.00")+" health.";
        }
        else
        {
            description += " No damage was taken by any involved.";
        }

        //relationship changes
        float relChange = 40f * Mathf.Pow(Mathf.Abs(robMultiplier), 1 - Mathf.InverseLerp(0f, 100f, target.attributes.perception));
        Relationship rel = dataController.RelationshipStorage[new RelationshipKey(target.id, performer.id)];
        rel.value = Mathf.Max(0f, rel.value - relChange);

        //happiness changes
        if(robMultiplier >= 0) 
        {
            performer.stats.happiness = Mathf.Min(100f, performer.stats.happiness + (1f + 9f * robMultiplier)); 
            target.stats.happiness = Mathf.Max(0f, target.stats.happiness - Mathf.Pow(1f + 9f * robMultiplier, Mathf.InverseLerp(0f, 100f, target.attributes.fortitude)));
        }
        else
        {
            performer.stats.happiness = Mathf.Max(0f, performer.stats.happiness - Mathf.Pow(1f + 9f * Mathf.Abs(robMultiplier), Mathf.InverseLerp(0f, 100f, performer.attributes.fortitude)));
            target.stats.happiness = Mathf.Min(100f, target.stats.happiness + (1f + 4f * Mathf.Abs(robMultiplier)));
        }

        //morality changes
        if(robMultiplier >= 0) performer.attributes.morality = Mathf.Max(0f, performer.attributes.morality - (1f + 9f * robMultiplier)); 
        else performer.attributes.morality = Mathf.Min(100f, performer.attributes.morality + 1f);

        bool wasPosPerf = false;
        bool wasPosRec = false;
        if(robMultiplier > 0) {wasPosPerf = true; wasPosRec = false;}
        else if(robMultiplier == 0) {wasPosPerf = false; wasPosRec = false;}
        else {wasPosPerf = false; wasPosRec = true;}

        float severity = 30f * Mathf.Abs(robMultiplier);
        dataController.historyManager.AddNPCMemory(name, description, severity, actionTime, performer.id, target.id, wasPosPerf, wasPosRec);
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NPC target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f); //better relationship makes it more likely that they'll succeed

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 100f, 0f, 2f, 0.25f, 5f)); weights.Add(0.8f); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.charisma, 0f, 100f, 1.25f, 0.75f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

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
    protected override float estimateSuccess(NPC performer, NPC target)
    {
        if(estSuccess != -1) return estSuccess;
        
        float baseSuccess;
        if(actSuccess == -1) baseSuccess = computeSuccess(performer, default);
        else baseSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(baseSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the NPC to complete this action
    protected override float getTimeToComplete(NPC performer, NPC target)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 100f, 0f, 0.25f, 2f, 5f)); weights.Add(0.8f); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.charisma, 0f, 100f, 0.75f, 1.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.perception, 0f, 100f, 0.25f, 3f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.dexterity, 0f, 100f, 0.25f, 3f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        float weightedTime = ActionMaths.ApplyWeightedMultipliers(baseTime, effectors, weights);
        weightedTime = ActionMaths.addChaos(weightedTime, dataController.worldManager.chaosModifier);

        timeToComplete = weightedTime;
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NPC target)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 100f, 0f, 0.25f, 2f, 5f)); weights.Add(0.8f); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.fortitude, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.charisma, 0f, 100f, 0.75f, 1.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.perception, 0f, 100f, 0.25f, 3f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.dexterity, 0f, 100f, 0.25f, 3f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        float weightedEnergy = ActionMaths.ApplyWeightedMultipliers(baseEnergy, effectors, weights);
        weightedEnergy = ActionMaths.addChaos(weightedEnergy, dataController.worldManager.chaosModifier);

        energyToComplete = weightedEnergy;
        return energyToComplete;
    }

    protected override bool isKnown(NPC performer)
    {
        float thisComplexity = complexity;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}