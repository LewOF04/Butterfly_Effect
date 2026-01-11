using UnityEngine;

[System.Serializable]
abstract class Action
{
    /*
    Accurately determine the exact utility of the 
    */
    private List<int> utilityAffectingTraits; //the traits that will have an impact on perceived utility
    private List<int> successAffectingTraits; //the traits that will have in impact on the action success
    private 
    private abstract double computeUtility(NPC performer, NPC receiver); //compute the imperical utility of the action 
    public abstract double estimateUtility(NPC performer, NPC receiver); //compute the performers perceived utility of the action
    public abstract void actionResult(NPC performer, NPC receiver); //compute the result of the action being performed
    public abstract bool successCalc(NPC performer, NPC receiver); //computer whether or not the action will be successful or fail
    public Action(){}
}