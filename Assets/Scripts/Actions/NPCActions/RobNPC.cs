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

        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.constituion, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f);
        effectors.Add(ActionMaths.calcExpMultiplier(performer.attributes.morality, 100f, 0f, 2f, 0.25f, 5f)); weights.Add(0.9f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); weights.Add(0.3f);

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(0.9f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

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

        Relationship thisRel = dataController.RelationShipStorage[new RelationshipKey(performer.id, target.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.wealth, 0f, 100f, 0.25f, 2f)); weights.Add(Mathf.InverseLerp(0f, 100f, performer.attributes.perception));
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(1 - Mathf.InverseLerp(0f, 100f, performer.attributes.fortitude));

        estUtility = ActionMaths.ApplyWeightedMultipliers(estUtility, effectors, weights);      

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        NPC target = dataController.BuildingStorage[receiver];

        float percentMulti = percentComplete / 100;
        string description = performer.npcName + " spent " + (percentMulti*timeToComplete).ToString("0.00") + " hours fixing the "+target.buildingName+" building.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, percentComplete);

        //add description and result levels based on success
        float fixMultiplier;
        if(successInfo.success == true) {
            description += "They successfully completed fixing the house ";
            
            if(successInfo.quality < 0.25f) {description += "but it was a bit of a shoddy job."; fixMultiplier = 0.7f;}
            else if(successInfo.quality < 0.5f) {description += "but they got paint on the scurting boards."; fixMultiplier = 0.85f;}
            else if(successInfo.quality < 0.75f) {description += "it looks as good as new."; fixMultiplier = 1f;}
            else {description += "and they even added some extra insulation."; fixMultiplier = 1.166f;}
        }
        else {
            description += "Their attempt to fix the house was not successful ";
            if(successInfo.quality < 0.25f) {description += "but it can be finished off another time."; fixMultiplier = 0.6f;}
            else if(successInfo.quality < 0.5f) {description += "but their attempt was appreciated by the inhabitants."; fixMultiplier = 0.4f;}
            else if(successInfo.quality < 0.75f) {description += "and they made the problems a little bit worse."; fixMultiplier = -0.2f;}
            else {description += "and they managed to break another part of the house."; fixMultiplier = -0.5f;}
        }

        if(percentComplete != 100f) description += " They had to stop working after doing "+percentComplete.ToString()+"% of the repair.";
        
        float actionTime = dataController.worldManager.gameTime + (24f - performer.timeLeft);

        //perform changes to stats/attributes
        description += "\n";

        float energyMinus = energyToComplete * percentMulti;
        performer.stats.energy -= energyMinus;
        description += "They spent "+energyMinus.ToString("0.00")+" energy, ";

        float timeMinus = timeToComplete * percentMulti;
        performer.timeLeft -= timeMinus;
        description += timeMinus.ToString("0.00")+" hours";

        float repairGain = baseConditionGain * fixMultiplier;
        target.condition += repairGain;
        description += ", altered the building condition by ";
        if(repairGain >= 0) description += "+";
        else description += "-";
        description+=repairGain.ToString("0.00");

        float repairCost = baseCost * percentMulti;
        performer.stats.wealth -= repairCost;
        description += " and cost "+repairCost.ToString("0.00");

        //calculate relationship changes to inhabitants
        if (!target.inhabitants.Contains(performer.id))
        {
            float totalRelChange = 0f;
            foreach(int inhabitantID in target.inhabitants)
            {
                Relationship rel = dataController.RelationshipStorage[new RelationshipKey(performer.id, inhabitantID)];
                float relChange = Mathf.InverseLerp(0f, 100f, rel.value) * 5f * fixMultiplier; //create the relationship change weighted by current relationship and outcome

                rel.value += relChange;

                if(rel.value > 100f) rel.value = 100f;
                if(rel.value < 0f) rel.value = 0f;

                totalRelChange += relChange;
            }

            description += " and altered their relationship with the inhabitants by ";
            float avgGain = totalRelChange / target.inhabitants.Count;
            if(avgGain >= 0) description += "+";
            else description += "-";
            description += avgGain.ToString("0.00")+" on average.";
        } else description += ".";

        bool wasPosPerf = false;
        bool wasPosRec = false;
        if(fixMultiplier > 0) {wasPosPerf = true; wasPosRec = true;}//if the fix wasn't negative then considered successful

        float severity;
        severity = ActionMaths.calcMultiplier(target.condition, 0f, 100f, 2f, 1f) * Mathf.Abs(fixMultiplier);
        dataController.historyManager.AddBuildingMemory(name, description, severity, actionTime, target.id, performer.id, wasPosPerf, wasPosRec);
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NPC target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //==================Relationship Outline==================
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, receiver.id)];
        effectors.Add(ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f); //better relationship makes it more likely that they'll succeed

        //==================Performer Stats==================
        effectors.Add(ActionMaths.calcExpMultiplier(performer.stats.condition, 100f, 0f, 2f, 0.25f, 5f)); weights.Add(0.8f); 
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Performer Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.fortitude, 0f, 100f, 0.25f, 2f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.charisma, 0f, 100f, 1.25f, 0.75f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, multipliers, weights);

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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.fortitude, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.charisma, 0f, 100f, 0.75f, 1.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.perception, 0f, 100f, 0.25f, 3f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.dexterity, 0f, 100f, 0.25f, 3f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        float weightedTime = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);
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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.fortitude, 0f, 100f, 2f, 0.25f)); weights.Add(0.5f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.perception, 0f, 100f, 2f, 0.25f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.dexterity, 0f, 100f, 2f, 0.25f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.strength, 0f, 100f, 2f, 0.25f)); weights.Add(0.9f);

        //==================Target Stats==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.condition, 0f, 100f, 2f, 0.5f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);

        //==================Target Attributes==================
        effectors.Add(ActionMaths.calcMultiplier(target.stats.charisma, 0f, 100f, 0.75f, 1.25f)); weights.Add(0.2f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.perception, 0f, 100f, 0.25f, 3f)); weights.Add(0.7f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.dexterity, 0f, 100f, 0.25f, 3f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcMultiplier(target.stats.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.9f);

        float weightedEnergy = ActionMaths.ApplyWeightedMultipliers(baseEnergy, multipliers, weights);
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