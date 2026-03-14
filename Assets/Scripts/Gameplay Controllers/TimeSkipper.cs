using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TimeSkipper : MonoBehaviour
{
    private SceneBuilder builder;
    private CamerMovement camMovement;  //camera script
    public Transform player; 
    private GameObject loadingScreen;
    private DataController dataController;
    public SimulationPlot simPlot = null; //produced sim plot
    private TextMeshProUGUI loadingInfoText;
    private Slider loadingSlider;
    private Canvas loadingCanvas;

    void Awake()
    {
        builder = FindFirstObjectByType<SceneBuilder>();
        camMovement = FindFirstObjectByType<CamerMovement>();
        player = FindFirstObjectByType<PlayerMovement>().gameObject.transform;
        loadingScreen = GameObject.Find("Loading Screen");
        dataController = DataController.Instance;

        loadingSlider = loadingScreen.GetComponentInChildren<Slider>();
        loadingCanvas = loadingScreen.GetComponent<Canvas>();
        loadingInfoText = loadingScreen.gameObject.transform.Find("Loading Info Text").GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        SimulationController.OnProgress += HandleSimulationProgress;
    }

    void OnDisable()
    {
        SimulationController.OnProgress -= HandleSimulationProgress;
    }

    private void HandleSimulationProgress(SimulationProgress progress)
    {
        loadingSlider.value = progress.progress;
        loadingInfoText.text = progress.message;
    }

    public void plotSim(float time)
    {
        StartCoroutine(plotTimeRoutine(time));
    }

    private IEnumerator plotTimeRoutine(float time)
    {
        Camera camera = FindFirstObjectByType<Camera>();

        //setup loading screen
        InputLocker.Lock(); //locks the input 
        loadingScreen.transform.position = camera.transform.position;
        loadingScreen.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
        loadingCanvas.enabled = true;

        loadingSlider.value = 0f;
        loadingInfoText.text = "Copying world state";
        yield return null;

        SimulationPlot simPlotInst = SimulationController.initiateSimulationPlot(time);
        loadingSlider.value = 10f;
        loadingInfoText.text = "Beginning simulation plotting";
        yield return null;

        yield return StartCoroutine(SimulationController.plotSimulationRoutine(simPlotInst));

        loadingSlider.value = 100f;
        loadingInfoText.text = "Plotting complete";
        yield return null;

        simPlot = simPlotInst;

        yield return new WaitForSeconds(1f);

        loadingScreen.transform.position = camera.transform.position;

        loadingCanvas.enabled = false;
        InputLocker.Unlock();
    }

    public void runFullSim()
    {
        StartCoroutine(runSimRoutine());
    }

    private IEnumerator runSimRoutine()
    {
        if(simPlot == null) {Debug.Log("There is no sim plot to run"); yield break;}

        Camera camera = FindFirstObjectByType<Camera>();

        //setup loading screen
        InputLocker.Lock(); //locks the input 
        loadingScreen.transform.position = camera.transform.position;
        loadingScreen.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
        loadingCanvas.enabled = true;

        loadingSlider.value = 0f;
        loadingInfoText.text = "Beginning simulation running on world state";
        yield return null;

        yield return StartCoroutine(SimulationController.runFullPlot(simPlot));

        loadingScreen.transform.position = camera.transform.position;

        simPlot = null;

        loadingInfoText.text = "Reloading world";
        yield return null;

        yield return StartCoroutine(reload(false));

        yield return new WaitForSeconds(1f);

        loadingCanvas.enabled = false;
        InputLocker.Unlock();
    }

    public IEnumerator reload(bool isIndependent)
    {
        Camera camera = FindFirstObjectByType<Camera>();
        if(isIndependent) 
        {
            //setup loading screen
            InputLocker.Lock(); //locks the input 
            loadingScreen.transform.position = camera.transform.position;
            loadingScreen.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
            loadingCanvas.enabled = true;

            loadingSlider.value = 0f;
            loadingInfoText.text = "";

            yield return null;
        }

        var sceneDecoration = GameObject.Find("SceneDecoration"); //get all of the scene decoration

        Destroy(sceneDecoration); //remove scene decoration
    
        builder.BuildScene(); //re-build the scene

        //reset camera position and player position
        Vector3 startPos = camMovement.startingPosition; 

        camMovement.transform.position = startPos;

        player.position = new Vector3(0,0,-0.5f);

        if (isIndependent)
        {
            loadingSlider.value = 100f;
            yield return null;

            loadingCanvas.enabled = false;
            InputLocker.Unlock();
        }
    }
}
