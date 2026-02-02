using UnityEngine;
using System.Collections.Generic;
public class ActionManager : MonoBehaviour
{
    public DataController dataController = DataController.Instance;

    public LoadActions()
    {
        List<BuildingAction> buildingActions = new Lists<BuildingAction>();
        List<NPCAction> npcActions = new List<NPCAction>();
        List<SelfAction> selfActions = new List<SelfAction>();
        List<EnvironmentAction> environmentActions = new List<EnvironmentAction>();
        Dictionary<string, IActionBase> ActionStorage = new Dictionart<string, IActionBase>();

        //get each class derived from the IActionBase interface
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException e)
                {
                    return e.Types.Where(t => t != null);
                }
            })
            .Where(t => typeof(IActionBase).IsAssignableFrom(t) &&
                        !t.IsAbstract &&
                        !t.IsInterface);


        //iterate over each type and save accordingly
        foreach (var t in types)
        {
            if (t.GetConstructor(Type.EmptyTypes) == null)
                continue;

            var instance = (IActionBase)Activator.CreateInstance(t);

            actionStorage[instance.name] = instance;

            // Also bucket them by category
            if (instance is BuildingAction ba) buildingActions.Add(ba);
            else if (instance is NPCAction na) npcActions.Add(na);
            else if (instance is SelfAction sa) selfActions.Add(sa);
            else if (instance is EnvironmentAction ea) environmentActions.Add(ea);
        }

        dataController.buildingActions = buildingActions;
        dataController.npcActions = npcActions;
        dataController.selfActions = selfActions;
        dataController.environmentActions = environmentActions;
        dataController.ActionStorage = ActionStorage;
    }
}