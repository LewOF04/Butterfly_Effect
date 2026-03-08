using System;
using System.Collections.Generic;
public struct ActionInfoWrapper : IEquatable<ActionInfoWrapper>
{
    public int currentActor;
    public int receiver;
    public float timeToComplete;
    public float energyToComplete;
    public bool known;
    public float estUtility;
    public float actUtility;
    public float estSuccess;
    public float actSuccess;
    public Action action;
    public ActionInfoWrapper(int currentActor, int receiver, float timeToComplete, float energyToComplete, bool? known, float estUtility, float actUtility, float estSuccess, float actSuccess, IAction action)
    {
        this.receiver = receiver;
        this.currentActor = currentActor;
        this.timeToComplete = timeToComplete;
        this.energyToComplete = energyToComplete;
        this.known = known;
        this.estUtility = estUtility;
        this.actUtility = actUtility;
        this.estSuccess = estSuccess;
        this.actSuccess = actSuccess;
        this.action = action;
    }
    public bool Equals(ActionInfoWrapper other)
    {
        if(receiver == other.receiver && currentActor == other.currentActor && action.name == other.action.name) return true;
        return false;
    }

    public override bool Equals(object other)
    {
        if(other is ActionInfoWrapper otherInf) return Equals(otherInf);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(receiver, currentActor, action.name);
    }

    public static bool operator == (ActionInfoWrapper a, ActionInfoWrapper b) => a.Equals(b);
    public static bool operator != (ActionInfoWrapper a, ActionInfoWrapper b) => !a.Equals(b);
}