using UnityEngine;
using System.Collections.Generic;
public class SpeechBubble : MonoBehaviour
{
    public NPC parentNPC;

    public string getSpeech()
    {
        if(parentNPC == null) return "";
        string speech;

        int speechOptions = 4;
        int choice = Random.Range(0, speechOptions+1);
        switch (choice)
        {
            case 0: speech = statsSpeech(parentNPC); break;
            case 1: speech = attributesSpeech(parentNPC); break;
            case 2: speech = relationshipSpeech(parentNPC); break;
            case 3: speech = memorySpeech(parentNPC); break;
            default: speech = traitSpeech(parentNPC); break;
        }
        return speech;
    }
    
    private static string statsSpeech(NPC parent)
    {
        string speech;
        int statOptions = 6;
        int choice = Random.Range(0, statOptions);
        switch (choice)
        {
            case 0: speech = conditionSpeech(parent); break;
            case 1: speech = nutritionSpeech(parent); break;
            case 2: speech = happinessSpeech(parent); break;
            case 3: speech = energySpeech(parent); break;
            case 4: speech = foodSpeech(parent); break;
            default: speech = wealthSpeech(parent); break;
        }
        return speech;
    }
    private static string attributesSpeech(NPC parent)
    {
        string speech;
        int attrOptions = 11;
        int choice = Random.Range(0, attrOptions);
        switch (choice)
        {
            case 0: speech = moralitySpeech(parent); break;
            case 1: speech = intelligenceSpeech(parent); break;
            case 2: speech = rationalitySpeech(parent); break;
            case 3: speech = fortitudeSpeech(parent); break;
            case 4: speech = charismaSpeech(parent); break;
            case 5: speech = aestheticSensSpeech(parent); break;
            case 6: speech = perceptionSpeech(parent); break;
            case 7: speech = dexteritySpeech(parent); break;
            case 8: speech = constitutionSpeech(parent); break;
            case 9: speech = wisdomSpeech(parent); break;
            default: speech = strengthSpeech(parent); break;
        }
        return speech;
    }
    private static string relationshipSpeech(NPC parent)
    {
        string speech = "";
        DataController dc = DataController.Instance;
        List<Relationship> relationships = dc.RelationshipPerNPCStorage[parent.id];
        if(relationships.Count == 0) return "There's nobody else in this world for me to have a relationship with...";
        int choice = Random.Range(0, relationships.Count);
        Relationship rel = relationships[choice];

        //TODO
        return speech;
    }
    private static string memorySpeech(NPC parent)
    {
        string speech = "";
        DataController dc = DataController.Instance;
        
        int eventType = Random.Range(0, 2);
        if(eventType == 0)
        {
            List<BuildingEvent> buildingEvents = dc.buildingEventsPerNPCStorage[parent.id];
            if(buildingEvents.Count == 0) return "I don't have any memories about buildings...";
            int choice = Random.Range(0, buildingEvents.Count);
            BuildingEvent theEvent = buildingEvents[choice];
            //TODO
        }
        else
        {
            List<NPCEvent> npcEvents = dc.eventsPerNPCStorage[parent.id];
            if(npcEvents.Count == 0) return "I don't have any memories about other people...";
            int choice = Random.Range(0, npcEvents.Count);
            NPCEvent theEvent = npcEvents[choice];
            //TODO
        }
        return speech;
    }
    private static string traitSpeech(NPC parent)
    {
        string speech;
        DataController dc = DataController.Instance;
        List<int> traits = parent.traits;
        if(traits.Count == 0) return "I don't have any interesting traits, I'm quite boring it seems.";
        int choice = Random.Range(0, traits.Count);
        int traitChoice = traits[choice];

        switch (traitChoice)
        {
            case 0: speech = "I really do fancy buying some things at the moment. I don't think I need them...actually yes I do, I want them and I need them."; break; //shopaholic
            case 1: speech = "I do have to question whether my neighbour likes me, I really hope that they do. I wonder how I could make them like me more if they don't."; break; //people pleaser
            case 2: speech = "Everyone here is unbelievably annoying, they won't stop bothering me and won't stop making stupid loud noise all of the time. I have half a mind to pop one of them in the face when I next get a chance!"; break; //hot-tempered
            case 3: speech = "I don't feel like I've done enough, there's not that much left to actually do but nonetheless I should really get on with it."; break; //workaholic
            case 4: speech = "There's a hell of a lot I would be better off doing now, orrr I could just sit around here doing nothing all day...that sounds more tempting actually."; break; //Lazy Bones
            case 5: speech = "The dark passenger is requesting that I take a stab at someone else. I don't think that they have any suspiciouns yet."; break; //Serial Killer
            case 6: speech = "You have twenty-four hours in a day, that means that if you get the minimum recommended amount of sleep, then you should spend 17 hours maxxing your potential."; break; //Life Guru
            case 7: speech = "Everything is messy and people keep leaving things around even when I specifically moved it out of the way."; break; //Neat Freak
            case 8: speech = "If anyone ever needs someone to lend a helping hand, I'd be more than happy to do so. My tools are gathering dust I haven't used them in so long."; break; //Handyman
            default: speech = "Has anyone been stocking away food where they shouldn't be? I bet that lady next door has got a secret stash somewhere I could look into..."; break; //gluttounous
        }
        return speech;
    }

    private static string conditionSpeech(NPC parent)
    {
        return "Condition";
    }
    private static string nutritionSpeech(NPC parent)
    {
        return "Nutrition";
    }
    private static string happinessSpeech(NPC parent)
    {
        return "Happiness";
    }
    private static string energySpeech(NPC parent)
    {
        return "Energy";
    }
    private static string foodSpeech(NPC parent)
    {
        return "Food";
    }
    private static string wealthSpeech(NPC parent)
    {
        return "Wealth";
    }
    private static string moralitySpeech(NPC parent)
    {
        return "Morality";
    }
    private static string intelligenceSpeech(NPC parent)
    {
        return "Intelligence";
    }
    private static string rationalitySpeech(NPC parent)
    {
        return "Rationality";
    }
    private static string fortitudeSpeech(NPC parent)
    {
        return "Fortitude";
    }
    private static string charismaSpeech(NPC parent)
    {
        return "Charisma";
    }
    private static string aestheticSensSpeech(NPC parent)
    {
        return "Aesthetic_Sens";
    }
    private static string perceptionSpeech(NPC parent)
    {
        return "Perception";
    }
    private static string dexteritySpeech(NPC parent)
    {
        return "Dexterity";
    }
    private static string constitutionSpeech(NPC parent)
    {
        return "Constitution";
    }
    private static string wisdomSpeech(NPC parent)
    {
        return "Wisdom";
    }
    private static string strengthSpeech(NPC parent)
    {
        return "Strength";
    }
}