using UnityEngine;
using System.Collections.Generic;
using System;

public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance;
    private IDataContainer dataController => DomainContext.DataController;

    private float startTime;
    private float currentTime;
    private float endTime;

    public SimulationPlot initiateSimulationPlot(float simulationTime)
    {
        DataControllerSnapshot dataSnapshot = new DataControllerSnapshot(); //copy game state
        DomainContext.DataController = dataSnapshot;

        SimulationPlot simPlot = new SimulationPlot(dataSnapshot); //create simulation plot

        startTime = dataSnapshot.World.gameTime;
        endTime = startTime + simulationTime;
        simPlot.startTime = startTime;
        simPlot.endTime = endTime;

        return simPlot;
    }

    public SimulationPlot plotSimulation(SimulationPlot simPlot)
    {
        
    }
}