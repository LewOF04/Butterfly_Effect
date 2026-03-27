using UnityEngine;
using System.Collections.Generic;

public static class SystemAction
{
    public static AgentUpdateInfo calcAgentUpdate(int receiver, float time)
    {
        IDataContainer dataController = DomainContext.DataController;
        if(!dataController.TryGetAgent(receiver, out var agent)) return null;

        string description = "At the game time "+time.ToString("0.00")+" the following passive effects have occurred:\n";

        /*==================Condition change==================*/
        /*Effected by Nutrition, Condition and constitution*/
        float conditionChange = 0f;
        //Nutrition
        const float condNutriThreshold = 20f;
        const float condNutriSwing = 1f; 
        float condNutriChange;
        if(agent.stats.nutrition < condNutriThreshold)
        {
            float multi = 2f * Mathf.InverseLerp(condNutriThreshold, 0f, agent.stats.nutrition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
            condNutriChange = -condNutriSwing * multi;
            description += "<u>Condition</u> negatively effected by low nutrition levels by <b>"+condNutriChange.ToString("0.00")+"</b>, and ";
        }
        else
        {
            float multi = 2f * Mathf.InverseLerp(condNutriThreshold, 100f, agent.stats.nutrition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
            condNutriChange = condNutriSwing * multi;
            description += "<u>Condition</u> positively effected by nutrition levels by <b>"+condNutriChange.ToString("0.00")+"</b>, and ";
        }
        conditionChange += condNutriChange;
        //condition
        const float condCondThreshold = 30f;
        const float condCondSwing = 0.5f;
        float condCondChange;
        if(agent.stats.condition < condCondThreshold)
        {
            float multi = 2f * Mathf.InverseLerp(condCondThreshold, 0f, agent.stats.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
            condCondChange = -condCondSwing * multi;
            description += "negatively effected by current condition level by <b>"+condCondChange.ToString("0.00")+"</b>. ";
        }
        else
        {
            float multi = 2f * Mathf.InverseLerp(condCondThreshold, 100f, agent.stats.condition) * Mathf.InverseLerp(0f, 100f, agent.attributes.constitution);
            condCondChange = condCondSwing * multi;
            description += "positively effected by current condition level by <b>"+condCondChange.ToString("0.00")+"</b>.\n";
        }
        conditionChange += condCondChange;

        /*==================Nutrition change==================*/
        /*Effected by Condition, constitution*/
        const float nutritionConsumption = 2.5f;
        float nutriMulti;
        if(agent.stats.condition < condCondThreshold) nutriMulti = Mathf.Pow(Mathf.InverseLerp(condCondThreshold, 0f, agent.stats.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution), 0.5f);
        else nutriMulti = 2f * Mathf.InverseLerp(100f, condCondThreshold, agent.stats.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
        float nutritionChange = nutriMulti * nutritionConsumption;
        description += "<u>Nutrition</u> decreased by <b>"+nutritionChange.ToString("0.00")+"</b> as a function of condition and constitution.\n";

        /*==================Happiness change==================*/
        /*Effected by happiness, fortitude, relationships, traits*/
        float happinessChange = 0f;
        //happiness
        const float happHappLowThresh = 40f;
        const float happHappHighThresh = 60f;
        const float happHappSwing = 1.5f;
        float happHappChange = 0f;
        if(agent.stats.happiness < happHappLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(happHappLowThresh, 0f, agent.stats.happiness) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
            happHappChange = -multi * happHappSwing;
            description += "<u>Happiness</u> negatively effected by current happiness by <b>"+happHappChange.ToString("0.00")+"</b>, ";
        } else if(agent.stats.happiness > happHappHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(happHappHighThresh, 100f, agent.stats.happiness) * Mathf.InverseLerp(0f, 100f, agent.attributes.fortitude);
            happHappChange = multi * happHappSwing;
            description += "<u>Happiness</u> positively effected by current happiness by <b>"+happHappChange.ToString("0.00")+"</b>, ";
        }
        happinessChange += happHappChange;
        //traits/relationships
        const float happRelSwing = 0.5f;
        const float happAvgRelThreshold = 50f;
        if (agent.traits.Contains(1)) //if they are a people pleaser
        {
            List<Relationship> relationships = dataController.RelationshipPerNPCStorage[agent.id];
            int totalRelNum = relationships.Count;

            if(totalRelNum > 0)
            {
                float totalRel = 0f;

                foreach(Relationship rel in relationships) totalRel += rel.value;

                float avgRel = totalRel / totalRelNum;
                float happRelChange;
                if(avgRel < happAvgRelThreshold)
                {
                    float multi = Mathf.InverseLerp(happAvgRelThreshold, 0f, agent.stats.happiness);
                    happRelChange = -multi * happRelSwing;
                    description += "negatively effected by being unliked whilst a people pleaser by <b>"+happRelChange.ToString("0.00")+"</b>, ";
                }
                else
                {
                    float multi = Mathf.InverseLerp(happAvgRelThreshold, 100f, agent.stats.happiness);
                    happRelChange = multi * happRelSwing;
                    description += "positively effected by being liked whilst a people pleaser by <b>"+happRelChange.ToString("0.00")+"</b>, ";
                }
                happinessChange += happRelChange;
            } 
        } 
        //world surroundings
        float happSurrThresh = 50f * (Mathf.InverseLerp(0f, 100f, agent.attributes.aesthetic_sensitivity) + 0.5f); //alter threshold based on the agents aesthetic sensitivty;
        const float happSurrSwing = 0.5f;
        float happSurrChange;
        if(dataController.TryGetBuilding(agent.parentBuilding, out IBuilding building)) //if they live in a house
        {   
            if(building.condition < happSurrThresh)
            {
                float multi = 2f * Mathf.InverseLerp(0f, happSurrThresh, building.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
                happSurrChange = -multi * happSurrSwing;
                description += "and negatively effected by their surroundings by "+happSurrChange.ToString("0.00")+".\n";
            }
            else
            {
                float multi = 2f * Mathf.InverseLerp(happSurrThresh, 100f, building.condition) * Mathf.InverseLerp(0f, 100f, agent.attributes.fortitude);
                happSurrChange = multi * happSurrSwing;
                description += "and positively effected by their surroundings by "+happSurrChange.ToString("0.00")+".\n";
            }
        }
        else //if they don't live in a house
        {
            float newHappSurrSwing = happSurrSwing * 2f;
            float multi = 2f * Mathf.InverseLerp(0f, 100f, agent.attributes.aesthetic_sensitivity) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
            happSurrChange = -multi * newHappSurrSwing;
            description += "and negatively effected by their surroundings by "+happSurrChange.ToString("0.00")+".\n";
        }
        happinessChange += happSurrChange;

        /*==================Energy change==================*/
        /*Effected by constitution and condition*/
        const float energyConsumption = 1f;
        float energyMulti = 2f * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution) * Mathf.InverseLerp(100f, 0f, agent.stats.condition);
        float energyChange = -energyConsumption * energyMulti;
        description += "<u>Energy</u> decreased by <b>"+energyChange.ToString("0.00")+"</b> as a function of constitution and condition.\n";

        /*==================Intelligence change==================*/
        float intelligenceChange = 0f;
        /*Effected by Wisdom, fortitude*/
        const float intelWisHighThresh = 70f;
        const float intelWisLowThresh = 30f;
        const float intelWisSwing = 0.5f;
        float intelWisChange = 0f;
        if(agent.attributes.intelligence > intelWisHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(intelWisHighThresh, 100f, agent.attributes.intelligence) * Mathf.InverseLerp(0f, 100f, agent.attributes.fortitude);
            intelWisChange = multi * intelWisSwing;
            description += "<u>Intelligence</u> increased by <b>"+intelWisChange.ToString("0.00")+"</b> as a function of wisdom and fortitude.\n";
        } else if(agent.attributes.intelligence < intelWisLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(intelWisLowThresh, 0f, agent.attributes.intelligence) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
            intelWisChange = -multi * intelWisSwing;
            description += "<u>Intelligence</u> decreased by <b>"+intelWisChange.ToString("0.00")+"</b> as a function of wisdom and fortitude.\n";
        }
        intelligenceChange += intelWisChange;

        /*==================Rationality change==================*/
        /*Effected by Nutrition, Condition, Fortitude*/
        float rationalityChange = 0f;
        //nutrition
        const float ratNutriLowThresh = 30f;
        const float ratNutriHighThresh = 70f;
        const float ratNutriSwing = 0.5f;
        float ratNutriChange = 0f;
        if(agent.stats.nutrition < ratNutriLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(ratNutriLowThresh, 0f, agent.stats.nutrition) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
            ratNutriChange = -multi * ratNutriSwing;
            description += "<u>Rationality</u> decreased by <b>"+ratNutriChange.ToString("0.00")+"</b> as a function of nutrition, and ";
        }else if(agent.stats.nutrition > ratNutriHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(ratNutriHighThresh, 100f, agent.stats.nutrition) * Mathf.InverseLerp(0f, 100f, agent.attributes.fortitude);
            ratNutriChange = multi * ratNutriSwing;
            description += "<u>Rationality</u> increased by <b>"+ratNutriChange.ToString("0.00")+"</b> as a function of nutrition, and ";
        }
        rationalityChange += ratNutriChange;
        //condition
        const float ratCondLowThresh = 30f;
        const float ratCondHighThresh = 70f;
        const float ratCondSwing = 0.5f;
        float ratCondChange = 0f;
        if(agent.stats.condition < ratCondLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(ratCondLowThresh, 0f, agent.stats.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.fortitude);
            ratCondChange = -multi * ratCondSwing;
            description += "decreased by <b>"+ratCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        } else if(agent.stats.condition > ratCondHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(ratCondHighThresh, 100f, agent.stats.condition) * Mathf.InverseLerp(0f, 100f, agent.attributes.fortitude);
            ratCondChange = multi * ratCondChange;
            description += "increased by <b>"+ratCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }
        rationalityChange += ratCondChange;

        /*==================Fortitude change==================*/
        float fortitudeChange = 0f;
        /*Effected by Happiness*/
        const float fortHappLowThresh = 20f;
        const float fortHappHighThresh = 80f;
        const float fortHappSwing = 1f;
        float fortHappChange = 0f;
        if(agent.stats.happiness < fortHappLowThresh)
        {
            float multi = Mathf.InverseLerp(fortHappLowThresh, 0f, agent.stats.happiness);
            fortHappChange = -multi * fortHappSwing;
            description += "<u>Fortitude</u> decreased by <b>"+fortHappChange.ToString("0.00")+"</b> as a function of happiness.\n";
        }else if (agent.stats.happiness > fortHappHighThresh)
        {
            float multi = Mathf.InverseLerp(fortHappHighThresh, 100f, agent.stats.happiness);
            fortHappChange = multi * fortHappSwing;
            description += "<u>Fortitude</u> increased by <b>"+fortHappChange.ToString("0.00")+"</b> as a function of happiness.\n";
        }
        fortitudeChange += fortHappChange;

        /*==================Charisma change==================*/
        float charismaChange = 0f;
        /*Effected by Condition*/
        const float charCondLowThresh = 10f;
        const float charCondHighThresh = 90f;
        const float charCondSwing = 0.5f;
        float charCondChange = 0f;
        if(agent.stats.condition < charCondLowThresh)
        {
            float multi = Mathf.InverseLerp(charCondLowThresh, 0f, agent.stats.condition);
            charCondChange = -multi * charCondSwing;
            description += "<u>Charsima</u> decreased by <b>"+charCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }else if(agent.stats.condition > charCondHighThresh)
        {
            float multi = Mathf.InverseLerp(charCondHighThresh, 100f, agent.stats.condition);
            charCondChange = multi * charCondSwing;
            description += "<u>Charisma</u> increased by <b>"+charCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }
        charismaChange += charCondChange;

        /*==================Perception change==================*/
        float perceptionChange = 0f;
        /*Effected by Fortitude*/
        const float percFortLowThresh = 10f;
        const float percFortHighThresh = 90f;
        const float percFortSwing = 0.5f;
        float percFortChange = 0f;
        if(agent.attributes.fortitude < percFortLowThresh)
        {
            float multi = Mathf.InverseLerp(percFortLowThresh, 0f, agent.attributes.fortitude);
            percFortChange = -multi * percFortSwing;
            description += "<u>Perception</u> decreased by <b>"+percFortChange.ToString("0.00")+"</b> as a function of fortitude.\n";
        }else if(agent.attributes.fortitude > percFortHighThresh)
        {
            float multi = Mathf.InverseLerp(percFortHighThresh, 100f, agent.attributes.fortitude);
            percFortChange = multi * percFortSwing;
            description += "<u>Perception</u> increased by <b>"+percFortChange.ToString("0.00")+"</b> as a function of fortitude.\n";
        }
        perceptionChange += percFortChange;

        /*==================Dexterity change==================*/
        float dexterityChange = 0f;
        /*Effected by Condition*/
        const float dexCondLowThresh = 10f;
        const float dexCondHighThresh = 90f;
        const float dexCondSwing = 0.5f;
        float dexCondChange = 0f;
        if(agent.stats.condition < dexCondLowThresh)
        {
            float multi = Mathf.InverseLerp(dexCondLowThresh, 0f, agent.stats.condition);
            dexCondChange = -multi * dexCondSwing;
            description += "<u>Dexterity</u> decreased by <b>"+dexCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }else if(agent.stats.condition > dexCondHighThresh)
        {
            float multi = Mathf.InverseLerp(dexCondHighThresh, 100f, agent.stats.condition);
            dexCondChange = multi * dexCondSwing;
            description += "<u>Dexterity</u> increased by <b>"+dexCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }
        dexterityChange += dexCondChange;

        /*==================constitution change==================*/
        float constitutionChange = 0f;
        /*Effected by Condition*/
        const float constCondLowThresh = 10f;
        const float constCondHighThresh = 90f;
        const float constCondSwing = 0.5f;
        float constCondChange = 0f;
        if(agent.stats.condition < constCondLowThresh)
        {
            float multi = Mathf.InverseLerp(constCondLowThresh, 0f, agent.stats.condition);
            constCondChange = -multi * constCondSwing;
            description += "<u>Constitution</u> decreased by <b>"+constCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }else if(agent.stats.condition > constCondHighThresh)
        {
            float multi = Mathf.InverseLerp(constCondHighThresh, 100f, agent.stats.condition);
            constCondChange = multi * constCondSwing;
            description += "<u>Constitution</u> increased by <b>"+constCondChange.ToString("0.00")+"</b> as a function of condition.\n";
        }
        constitutionChange += constCondChange;
        

        /*==================Wisdom change==================*/
        float wisdomChange = 0f;
        /*Effected by Fortitude*/
        const float wisFortLowThresh = 20f;
        const float wisFortHighThresh = 80f;
        const float wisFortSwing = 0.5f;
        float wisFortChange = 0f;
        if(agent.attributes.fortitude < wisFortLowThresh)
        {
            float multi = Mathf.InverseLerp(wisFortLowThresh, 0f, agent.attributes.fortitude);
            wisFortChange = -multi * wisFortSwing;
            description += "<u>Wisdom</u> decreased by <b>"+wisFortChange.ToString("0.00")+"</b> as a function of fortitude.\n";
        }else if(agent.attributes.fortitude > wisFortHighThresh)
        {
            float multi = Mathf.InverseLerp(wisFortHighThresh, 100f, agent.attributes.fortitude);
            wisFortChange = multi * wisFortSwing;
            description += "<u>Wisdom</u> increased by <b>"+wisFortChange.ToString("0.00")+"</b> as a function of fortitude.\n";
        }
        wisdomChange += wisFortChange;

        /*==================Strength change==================*/
        float strengthChange = 0f;
        /*Effected by Condition, Nutrition, Constitution*/
        //condition
        const float strCondLowThresh = 10f;
        const float strCondHighThresh = 90f;
        const float strCondSwing = 0.5f;
        float strCondChange = 0f;
        if(agent.stats.condition < strCondLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(strCondLowThresh, 0f, agent.stats.condition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
            strCondChange = -multi * strCondSwing;
            description += "<u>Strength</u> decreased by <b>"+strCondChange.ToString("0.00")+"</b> as a function of condition, and ";
        }else if(agent.stats.condition > strCondHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(strCondHighThresh, 100f, agent.stats.condition) * Mathf.InverseLerp(0f, 100f, agent.attributes.constitution);
            strCondChange = multi * strCondSwing;
            description += "<u>Strength</u> increased by <b>"+strCondChange.ToString("0.00")+"</b> as a function of condition, and ";
        }
        strengthChange += strCondChange;
        //nutrition
        const float strNutriLowThresh = 10f;
        const float strNutriHighThresh = 90f;
        const float strNutriSwing = 0.5f;
        float strNutriChange = 0f;
        if(agent.stats.nutrition < strNutriLowThresh)
        {
            float multi = 2f * Mathf.InverseLerp(strNutriLowThresh, 0f, agent.stats.nutrition) * Mathf.InverseLerp(100f, 0f, agent.attributes.constitution);
            strNutriChange = -multi * strNutriSwing;
            description += "decreased by <b>"+strNutriChange.ToString("0.00")+"</b> as a function of nutrition.\n";
        }else if(agent.stats.nutrition > strNutriHighThresh)
        {
            float multi = 2f * Mathf.InverseLerp(strNutriHighThresh, 100f, agent.stats.nutrition) * Mathf.InverseLerp(0f, 100f, agent.attributes.constitution);
            strNutriChange = multi * strNutriSwing;
            description += "increased by <b>"+strNutriChange.ToString("0.00")+"</b> as a function of nutrition.\n";
        }
        strengthChange += strNutriChange;

        return new AgentUpdateInfo(description, conditionChange, nutritionChange, happinessChange, energyChange, intelligenceChange, rationalityChange, 
                                   fortitudeChange, charismaChange, perceptionChange, constitutionChange, wisdomChange, strengthChange, receiver, time);
    }

    public static void performAgentUpdate(AgentUpdateInfo updateInfo)
    {
        if(!DomainContext.DataController.TryGetAgent(updateInfo.receiver, out var agent)) return;
        agent.stats.condition = Mathf.Clamp(agent.stats.condition + updateInfo.conditionChange, 0f, 100f);
        agent.stats.nutrition = Mathf.Clamp(agent.stats.nutrition + updateInfo.nutritionChange, 0f, 100f);
        agent.stats.happiness = Mathf.Clamp(agent.stats.happiness + updateInfo.happinessChange, 0f, 100f);
        agent.stats.energy = Mathf.Clamp(agent.stats.energy + updateInfo.energyChange, 0f, 100f);
        agent.attributes.intelligence = Mathf.Clamp(agent.attributes.intelligence + updateInfo.intelligenceChange, 0f, 100f);
        agent.attributes.rationality = Mathf.Clamp(agent.attributes.rationality + updateInfo.rationalityChange, 0f, 100f);
        agent.attributes.fortitude = Mathf.Clamp(agent.attributes.fortitude + updateInfo.fortitudeChange, 0f, 100f);
        agent.attributes.charisma = Mathf.Clamp(agent.attributes.charisma + updateInfo.charismaChange, 0f, 100f);
        agent.attributes.perception = Mathf.Clamp(agent.attributes.perception + updateInfo.perceptionChange, 0f, 100f);
        agent.attributes.constitution = Mathf.Clamp(agent.attributes.constitution + updateInfo.constitutionChange, 0f, 100f);
        agent.attributes.wisdom = Mathf.Clamp(agent.attributes.wisdom + updateInfo.wisdomChange, 0f, 100f);
        agent.attributes.strength = Mathf.Clamp(agent.attributes.strength + updateInfo.strengthChange, 0f, 100f);
    }
    
    public static BuildingUpdateInfo calcBuildingUpdate(int receiver, float time)
    {
        if(!DomainContext.DataController.TryGetBuilding(receiver, out var building)) return null;
        float conditionChange = 0f;
        const float condThresh = 50f;
        const float condSwing = 0.5f;
        float multi;
        if(building.condition < condThresh) multi = -Mathf.InverseLerp(0f, condThresh, building.condition);
        else multi = Mathf.InverseLerp(condThresh, 100f, building.condition) * condSwing;
        
        conditionChange += multi * condSwing;
        return new BuildingUpdateInfo(conditionChange, receiver, time);
    }
    public static void performBuildingUpdate(BuildingUpdateInfo updateInfo)
    {
        if(!DomainContext.DataController.TryGetBuilding(updateInfo.receiver, out var building)) return;
        building.condition = Mathf.Clamp(building.condition + updateInfo.conditionChange, 0f, 100f);
    }

    public static AgentDeathInfo calcAgentDeath(int receiver, float time)
    {
        Debug.Log("CALCULATING DEATH");
        IDataContainer dataController = DomainContext.DataController;
        
        Dictionary<int, float> happinessChanges = new Dictionary<int, float>();
        
        if(!dataController.TryGetAgent(receiver, out var agent)) return null;
        string description = agent.fullName + " died at "+time.ToString("0.00")+" for those that like him it will cause sadness, for others maybe not so much.";

        List<Relationship> relationships = dataController.RelationshipPerNPCStorage[receiver];
        foreach(Relationship rel in relationships)
        {
            int otherAgent;
            if(rel.key.npcA == receiver) otherAgent = rel.key.npcB;
            else otherAgent = rel.key.npcA;

            float change;
            if(rel.value < 60f && rel.value > 40f) change = Mathf.Lerp(-5f, 5f, Mathf.InverseLerp(60f, 40f, rel.value));
            if(rel.value < 40f) change = Mathf.Lerp(5f, 20f, Mathf.InverseLerp(40f, 0f, rel.value));
            else change = Mathf.Lerp(-20f, -5f, Mathf.InverseLerp(100f, 60f, rel.value));

            happinessChanges[otherAgent] = change;
        }

        return new AgentDeathInfo(receiver, time, description, happinessChanges);
    }
    public static void performAgentDeath(AgentDeathInfo updateInfo)
    {
        Debug.Log("PERFORMING DEATH");
        IDataContainer dataController = DomainContext.DataController;
        if(dataController is DataController dinst) Debug.Log("Performing agent death on: instance");
        else Debug.Log("Performing agent death on: snapshot");
        if(!dataController.TryGetAgent(updateInfo.receiver, out var agent)) return;

        agent.isAlive = false;

        if(!dataController.TryGetBuilding(agent.parentBuilding, out var building)) return;
        building.inhabitants.Remove(agent.id);

        agent.parentBuilding = -1;

        foreach(var kvp in updateInfo.happinessChanges)
        {
            int id = kvp.Key;
            float change = kvp.Value;
            
            if(!dataController.TryGetAgent(id, out var otherAgent)) return;

            otherAgent.stats.happiness = Mathf.Clamp(otherAgent.stats.happiness + change, 0f, 100f);
        }
    }
}

public interface IUpdateInfo : ISimEvent
{
    int receiver {get; set;}
    float eventTime {get; set;}
    public string description {get;}
    void performUpdate();
}

public class BuildingUpdateInfo : IUpdateInfo
{
    public int receiver {get; set;}
    public float eventTime {get; set;}
    public float conditionChange;
    public string description {get => "";}
    public BuildingUpdateInfo(float iConditionChange, int iReceiver, float iTime)
    {
        eventTime = iTime;
        receiver = iReceiver;
        conditionChange = iConditionChange;
    }
    public void performUpdate() => SystemAction.performBuildingUpdate(this);
}
public class AgentUpdateInfo : IUpdateInfo
{
    public int receiver {get; set;}
    public float eventTime {get; set;}
    public string description {get;}
    public float conditionChange;
    public float nutritionChange;
    public float happinessChange;
    public float energyChange;
    public float intelligenceChange;
    public float rationalityChange;
    public float fortitudeChange;
    public float charismaChange;
    public float perceptionChange;
    public float constitutionChange;
    public float wisdomChange;
    public float strengthChange;

    public AgentUpdateInfo(string iDescription, float iConditionChange, float iNutritionChange, float iHappinessChange, 
                           float iEnergyChange, float iIntelligenceChange, float iRationalityChange, float iFortitudeChange,
                           float iCharismaChange, float iPerceptionChange, float iConstitutionChange, float iWisdomChange,
                           float iStrengthChange, int iReceiver, float iTime)
    {
        eventTime = iTime;
        description = iDescription;
        conditionChange = iConditionChange;
        nutritionChange = iNutritionChange;
        happinessChange = iHappinessChange;
        energyChange = iEnergyChange;
        intelligenceChange = iIntelligenceChange;
        rationalityChange = iRationalityChange;
        fortitudeChange = iFortitudeChange;
        charismaChange = iCharismaChange;
        perceptionChange = iPerceptionChange;
        constitutionChange = iConstitutionChange;
        wisdomChange = iWisdomChange;
        strengthChange = iStrengthChange;
        receiver = iReceiver;
    }

    public void performUpdate() => SystemAction.performAgentUpdate(this);
}
public class AgentDeathInfo : IUpdateInfo
{
    public int receiver {get; set;}
    public float eventTime {get; set;}
    public string description {get;}
    public Dictionary<int, float> happinessChanges;
    public AgentDeathInfo(int rec, float time, string desc, Dictionary<int, float> happChanges)
    {
        receiver = rec;
        eventTime = time;
        description = desc;
        happinessChanges = happChanges;
    }
    public void performUpdate() => SystemAction.performAgentDeath(this);
}