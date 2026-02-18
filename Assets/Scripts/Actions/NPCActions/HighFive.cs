using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using NoTarget = System.ValueTuple;

[Preserve]
public class HighFive : NPCAction
{
    public HighFive() : base('N'){}
    private float baseRelImprove = 5f;

    public override char actionType => 'N';
    public override string name => "High Five";
    public override string baseDescription => "Sometimes it's just nice to be happy, give them a high-five.";

    protected override float baseTime => 0.001f;
    protected override float baseEnergy => 1f;
    protected override float complexity => 0.5f;
    protected override float baseUtility => 50f;

    protected override List<int> utilityPosTraits => new List<int>{1, 5}; 
    protected override List<int> utilityNegTraits => new List<int>{2};
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
        effectors.Add(ActionMaths.calcMultiplier(performer.stats.happiness, 0f, 100f, 0.25f, 2f)); weights.Add(1 - Mathf.InverseLerp(0f, 100f, performer.attributes.fortitude));   

        //energy and time effectors
        effectors.Add(ActionMaths.scarcityMultiplier(performer.stats.energy - energyToComplete, 0f, 100f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.scarcityMultiplier(performer.timeLeft - timeToComplete, 0f, 24f, 0.1f, 2f)); weights.Add(0.8f);
        effectors.Add(ActionMaths.calcMultiplier(actSuccess, 0f, 100f, 0.25f, 2f)); weights.Add(0.2f);

        //add a weighter based on the npcs relationship with the other
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)]; //get the relationship between these two npcs
        effectors.Add(ActionMaths.calcExpMultiplier(thisRel.value, 0f, 100f, 2f, 0.5f, 3f)); weights.Add(0.9f); //the relatively lower the relationship the better it would be to increase it

        effectors.Add(ActionMaths.calcMultiplier(baseRelImprove, 0f, 100f, 1f, 1.5f)); weights.Add(0.8f); //adds multiplier to weight benefit

        actUtility = ActionMaths.ApplyWeightedMultipliers(baseUtility, effectors, weights);
        return actUtility;
    }

    //compute the performers perceived utility of the action
    protected override float estimateUtility(NPC performer, NPC target)
    {
        if(estUtility != -1) return estUtility;
        
        if(actUtility == -1) actUtility = computeUtility(performer, default);

        List<float> effectors = new List<float>();
        List<float> weights = new List<float>();

        //add a weighter based on the npcs relationship with the other
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)]; //get the relationship between these two npcs
        effectors.Add(ActionMaths.calcExpMultiplier(thisRel.value, 0f, 100f, 0.1f, 4f, 5f)); weights.Add(Mathf.Clamp(Mathf.InverseLerp(0f, 100f, performer.attributes.wisdom), 0.4f, 0.9f)); //the relatively higher the relationship the better it would be to increase it

        estUtility = ActionMaths.ApplyWeightedMultipliers(actUtility, effectors, weights);

        estUtility = ActionMaths.addTraitWeights(performer, actUtility, utilityPosTraits, true);
        estUtility = ActionMaths.addTraitWeights(performer, estUtility, utilityNegTraits, false); 

        estUtility = ActionMaths.rationalityNoise(estUtility, performer.attributes.rationality);
        return estUtility;
    }

    protected override void innerPerformAction(float _)
    {
        NPC performer = dataController.NPCStorage[currentActor];
        NPC target = dataController.NPCStorage[receiver];

        string description = performer.npcName + " spent " + timeToComplete.ToString("0.00") + " giving "+target.npcName+" a high five.";
        ActionResult successInfo = ActionMaths.calcActionSuccess(actSuccess, 100f);

        bool wasPosRec;
        bool wasPosPerf;
        float relMultiplier;

        //determine results based on the success of the action
        if(successInfo.success == true) {
            description += "They successfully had a high five ";
            
            if(successInfo.quality < 0.25f) {
                description += "but it was an awkward collision of pinkies."; 
                relMultiplier = 0.33f;
                wasPosPerf = true;
                wasPosRec = true;
            }
            else if(successInfo.quality < 0.5f) {
                description += "but their hands stung afterwards."; 
                relMultiplier = 0.66f;
                wasPosPerf = true;
                wasPosRec = true;
            }
            else if(successInfo.quality < 0.75f) {
                description += "enthusiastically jumping in the air as they did."; 
                relMultiplier = 1f;
                wasPosPerf = true;
                wasPosRec = true;
            }
            else {
                description += "and the sound produced from it reverberated for all to hear."; 
                relMultiplier = 1.33f;
                wasPosPerf = true;
                wasPosRec = true;
            }
        }
        else {
            description += "Their high five was not a success ";
            if(successInfo.quality < 0.25f) {
                description += "they somehow managed to miss hands entirely."; 
                relMultiplier = -0.33f;
                wasPosPerf = false;
                wasPosRec = false;
            }
            else if(successInfo.quality < 0.5f) {
                description += "and they were left with the embarrassment of a poor attempt."; 
                relMultiplier = -0.66f;
                wasPosPerf = false;
                wasPosRec = false;
            }
            else if(successInfo.quality < 0.75f) {
                description += target.npcName+" looked at "+performer.npcName+" with disgust when they attempted it."; 
                relMultiplier = -1f;
                wasPosPerf = false;
                wasPosRec = true;
            }
            else {
                description += "they were too scared to attempt it with "+target.npcName; 
                relMultiplier = -1.33f;
                wasPosPerf = false;
                wasPosRec = true;
            }
        }
        
        float actionTime = dataController.worldManager.gameTime + (24f - performer.timeLeft);

        description += "\n";

        performer.stats.energy -= energyToComplete;
        description += "They spent "+energyToComplete.ToString("0.00")+" energy, ";

        performer.timeLeft -= timeToComplete;
        description += timeToComplete.ToString("0.00")+" hours ";

        //determine change to relationship
        Relationship rel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)];
        float relGain = 5f * relMultiplier;
        rel.value = Mathf.Min(100f, rel.value + relGain);
        description += "and altered their relationship with "+target.npcName+" by ";
        if(relGain >= 0) description += "+";
        else description += "-";
        description += relGain.ToString("0.00")+".";

        //determine severity based on relationship
        float severity;
        if(successInfo.success == true) severity = ActionMaths.calcMultiplier(rel.value, 0f, 100f, 2f, 0.25f) * Mathf.Abs(relMultiplier); //the worse their relationship the more positive the outcome
        else severity = ActionMaths.calcMultiplier(rel.value, 0f, 100f, 0.25f, 2f) * Mathf.Abs(relMultiplier); //the worse the relationship the less negative the outcome
        
        dataController.historyManager.AddNPCMemory(name, description, severity, actionTime, performer.id, target.id, wasPosPerf, wasPosRec);
    }

    //computer the likelihood this action will be a success
    protected override float computeSuccess(NPC performer, NPC target)
    {
        if(actSuccess != -1) return actSuccess;

        List<float> multipliers = new List<float>();
        List<float> weights = new List<float>();

        //dexterity, constitution, energy, nutrition, condition
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.dexterity, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f); //dexterity multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.attributes.constitution, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f); //constitution multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.energy, 0f, 100f, 0.25f, 2f)); weights.Add(0.8f); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 0.25f, 2f)); weights.Add(0.1f); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 0.25f, 2f)); weights.Add(0.4f); //condition multiplier

        //add a weighter based on the npcs relationship with the other
        Relationship thisRel = dataController.RelationshipStorage[new RelationshipKey(performer.id, target.id)]; //get the relationship between these two npcs
        multipliers.Add(ActionMaths.calcExpMultiplier(thisRel.value, 100f, 0f, 4f, 0.1f, 5f)); weights.Add(0.9f); //the relatively higher the relationship the better it would be to increase it

        actSuccess = ActionMaths.ApplyWeightedMultipliers(60f, multipliers, weights);
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

        timeToComplete = ActionMaths.addChaos(baseTime, dataController.worldManager.chaosModifier);
        return timeToComplete;
    }

    //calculate how much energy it would take the NPC to compelete this action
    protected override float getEnergyToComplete(NPC performer, NPC target)
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
        multipliers.Add(ActionMaths.calcExpMultiplier(performer.stats.energy, 100f, 0f, 0.1f, 1f, 5f)); //energy multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.nutrition, 0f, 100f, 2f, 0.25f)); //nutrition multiplier
        multipliers.Add(ActionMaths.calcMultiplier(performer.stats.condition, 0f, 100f, 2f, 0.25f)); //condition multiplier

        return multipliers;
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