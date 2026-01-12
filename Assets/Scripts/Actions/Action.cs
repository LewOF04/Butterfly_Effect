using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class Action
{
    /*
    Accurately determine the exact utility of the 
    */
    public List<int> utilityAffectingTraits; //the traits that will have an impact on perceived utility
    public List<int> successAffectingTraits; //the traits that will have in impact on the action success
    public abstract double computeUtility(NPC performer, NPC receiver); //compute the imperical utility of the action 
    public abstract double estimateUtility(NPC performer, NPC receiver); //compute the performers perceived utility of the action
    public abstract void actionResult(NPC performer, NPC receiver); //compute the result of the action being performed
    public abstract bool successCalc(NPC performer, NPC receiver); //computer whether or not the action will be successful or fail
    public abstract bool isKnown(NPC performer, NPC receiver); //check that this action would be known to the NPC
    public Action(){}
}