using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
public class ActionManager : MonoBehaviour
{
    public DataController dataController = DataController.Instance;

    public void LoadActions()
    {
        List<BuildingAction> buildingActions = new List<BuildingAction>();
        List<NPCAction> npcActions = new List<NPCAction>();
        List<SelfAction> selfActions = new List<SelfAction>();
        List<EnvironmentAction> environmentActions = new List<EnvironmentAction>();
        Dictionary<string, IActionBase> actionStorage = new Dictionary<string, IActionBase>();

        //get each class derived from the IActionBase interface
        var types = AppDomain.CurrentDomain.GetAssemblies() //in the current game get all the dynamically linked libraries (including the types we've created)
            .SelectMany(a => //for each assembly
            {
                try {return a.GetTypes();} //try to get all of the types in the assembly
                catch (System.Reflection.ReflectionTypeLoadException e) //if we're unable to get a type then we can forget it
                {
                    return e.Types.Where(t => t != null); //return all the valid types in the assembly
                }
            })
            .Where(t => typeof(IActionBase).IsAssignableFrom(t) && //limit to those that are a derivation of IActionBase and isn't abstract or an interface
                        !t.IsAbstract &&
                        !t.IsInterface);


        //iterate over each type and save accordingly
        foreach (var t in types)
        {
            if (t.GetConstructor(Type.EmptyTypes) == null) //try to get the constructor for this type
                continue;

            var instance = (IActionBase)Activator.CreateInstance(t); //create an instance of it

            actionStorage[instance.name] = instance; //store is in our action storage by the actions name

            //save class instnaces into correct storage
            if (instance is BuildingAction ba) buildingActions.Add(ba);
            else if (instance is NPCAction na) npcActions.Add(na);
            else if (instance is SelfAction sa) selfActions.Add(sa);
            else if (instance is EnvironmentAction ea) environmentActions.Add(ea);
        }

        dataController.buildingActions = buildingActions;
        dataController.npcActions = npcActions;
        dataController.selfActions = selfActions;
        dataController.environmentActions = environmentActions;
        dataController.ActionStorage = actionStorage;
    }
}