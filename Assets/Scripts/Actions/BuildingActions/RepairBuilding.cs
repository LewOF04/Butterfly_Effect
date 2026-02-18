using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class RepairBuilding : BuildingAction
{
    private const float baseCost = 5f;
    private const float baseConditionGain = 15f;
    public RepairBuilding() : base('B'){}

    public override char actionType => 'B';
    public override string name => "Repair Building";
    public override string baseDescription => "Damaged and ugly houses are not nice to see for anyone, therefore you can repair them!";

    protected override float baseTime => 10f;
    protected override float baseEnergy => 50f;
    protected override float complexity => 40f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{7, 8}; 
    protected override List<int> utilityNegTraits => new List<int>{4};
    protected override List<int> successPosTraits => new List<int>{8}; 
    protected override List<int> successNegTraits => new List<int>{};

    //compute the empirical utility of the action
    protected override float computeUtility(NPC performer, Building target)
    {
        if(actUtility != -1) return actUtility;

        if(timeToComplete == -1) getTimeToComplete(performer, target);
        if(energyToComplete == -1) getEnergyToComplete(performer, target);
        if(actSuccess == -1) computeSuccess(performer, target);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //npc stat/attribute effectors
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.perception, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f);
        effectors.Add(ActionMaths.calcMultiplier(performer.attributes.aesthetic_sensitivity, 0f, 100f, 0.25f, 2f)); weights.Add(0.6f);
        effectors.Add(ActionMaths.calcExpMultiplier(target.condition, 0f, 100f, 5f, 0.25f, 5f)); weights.Add(0.8f); //the worse the condition the more important to fix

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.wealth - baseCost, 0f, 100f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        effectors.Add(ActionMaths.calcExpMultiplier(target.condition, 100f, 0f, 0.25f, 5f, 5f)); weights.Add(0.9f);

        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, Building target)
    {
        if(estUtility != -1) return estUtility;
        
        if(actUtility == -1) actUtility = computeUtility(performer, default);

        estUtility = ActionMaths.addTraitWeights(performer, actUtility, utilityPosTraits, true);
        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityNegTraits, false); 

        if(target.id != performer.parentBuilding){ 
            estUtility -= estUtility * 0.3f; //utility of someone else's house is lower
            //if it is not their house, weight the benefit by how much they like the inhabitants
            foreach(int npcID in target.inhabitants)
            {
                NPC inhabitantNPC = dataController.NPCStorage[npcID];
                Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(npcID, performer.id)];
                float relLevel = ActionMaths.calcMultiplier(thisRel.value, 0f, 100f, -1f, 2f);

                estUtility += estUtility * (0.1f * Mathf.Clamp(Mathf.InverseLerp(0f, 100f, relLevel), 0.1f, 0.9f));
            }
        }

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float percentComplete)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        Building target = dataController.BuildingStorage[receiver];

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
        target.condition = Mathf.Min(100f, target.condition + repairGain);
        description += ", altered the building condition by ";
        if(repairGain >= 0) description += "+";
        else description += "-";
        description+=repairGain.ToString("0.00");

        float repairCost = baseCost * percentMulti;
        performer.stats.wealth = Mathf.Max(0f, performer.stats.wealth - repairCost);
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

        float severity = ActionMaths.calcMultiplier(target.condition, 0f, 100f, 2f, 1f) * Mathf.Abs(fixMultiplier);
        dataController.historyManager.AddBuildingMemory(name, description, severity, actionTime, target.id, performer.id, wasPosPerf, wasPosRec);
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, Building target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> multipliers = new List<float>();
        List<float> weights = new List<float>();

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.3f); //dexterity multiplier
        multipliers.Add(ActionMaths.calcExpMultiplier(performer.attributes.constitution, 100f, 0f, 1.5f, 0.25f, 4f)); weights.Add(0.3f); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 0.25f, 1.75f)); weights.Add(0.8f); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 1.5f)); weights.Add(0.2f); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 1.5f)); weights.Add(0.6f); //condition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 0.25f, 2f)); weights.Add(0.7f); //strength multiplier
        float weightedSucc = ActionMaths.ApplyWeightedMultipliers(50f, multipliers, weights);

        if(performer.traits.Contains(8)) weightedSucc += weightedSucc * 0.5f;

        actSuccess = weightedSucc;
        return actSuccess;
    }

    //compute the estimated chance this action will be a succss from the performers perspective
    protected override float estimateSuccess(NPC performer, Building target)
    {
        if(estSuccess != -1) return estSuccess;
        
        float baseSuccess;
        if(actSuccess == -1) baseSuccess = computeSuccess(performer, default);
        else baseSuccess = actSuccess;

        estSuccess = ActionMaths.rationalityNoise(baseSuccess, performer.attributes.rationality);
        return estSuccess;
    }

    //calculate how much time it would take for the NPC to complete this action
    protected override float getTimeToComplete(NPC performer, Building target)
    {
        if(timeToComplete != -1) return timeToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.7f, 0.7f, 0.5f, 0.2f, 0.2f, 0.6f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseTime, multipliers, weights);

        if(performer.traits.Contains(8)) weightedBase -= weightedBase * 0.4f;

        timeToComplete = ActionMaths.addChaos(weightedBase, dataController.worldManager.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, Building target)
    {
        if(energyToComplete != -1) return energyToComplete;

        List<float> multipliers = getTimeAndEnergyMultipliers(performer);
        List<float> weights = new List<float>{0.3f, 0.3f, 0.8f, 0.7f, 0.5f, 0.6f};

        float weightedBase = ActionMaths.ApplyWeightedMultipliers(baseEnergy, multipliers, weights);

        if(performer.traits.Contains(8)) weightedBase -= weightedBase * 0.2f;

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
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.strength, 0f, 100f, 2f, 0.25f)); //strength multiplier

        return multipliers;
    }

    protected override bool isKnown(NPC performer)
    {
        float thisComplexity = complexity;
        if(performer.traits.Contains(8)) thisComplexity -= thisComplexity * 0.3f;

        //either the action is known because they're smart enough
        if (complexity <= performer.attributes.intelligence) return true;

        //or they have memory of a similar event
        List<NPCEvent> npcEvents = dataController.eventsPerNPCStorage[performer.id];
        foreach(NPCEvent thisEvent in npcEvents)  if(thisEvent.actionName == name) return true;

        return false;
    } 
}