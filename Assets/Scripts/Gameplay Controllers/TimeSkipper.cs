using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TimeSkipper : MonoBehaviour
{
    private SceneBuilder builder;
    private CamerMovement camMovement;  //camera script
    public Transform player; 
    private GameObject loadingScreen;
    private DataController dataController;

    void Awake()
    {
        builder = FindFirstObjectByType<SceneBuilder>();
        camMovement = FindFirstObjectByType<CamerMovement>();
        player = FindFirstObjectByType<PlayerMovement>().gameObject.transform;
        loadingScreen = GameObject.Find("Loading Screen");
        dataController = DataController.Instance;
    }

    public void skipTime(int time)
    {
        StartCoroutine(SkipTimeRoutine(time));
    }
    IEnumerator SkipTimeRoutine(int time)
    {
        Camera camera = FindFirstObjectByType<Camera>();

        //setup loading screen
        InputLocker.Lock(); //locks the input 
        Slider loadingSlider = loadingScreen.GetComponentInChildren<Slider>();
        Canvas canvas = loadingScreen.GetComponent<Canvas>();
        loadingScreen.transform.position = camera.transform.position;
        loadingScreen.transform.position += new Vector3(0.0f, 0.0f, 0.5f);
        canvas.enabled = true;
        loadingSlider.value = 0f;
        
        yield return StartCoroutine(reloadWorld()); //wait for reload to complete
        loadingScreen.transform.position = camera.transform.position;
        loadingScreen.transform.position += new Vector3(0.0f, 0.0f, 0.5f);

        Dictionary<int, NPC> npcs = dataController.NPCStorage;
        List<int> npcKeys = new List<int>(npcs.Keys);
        foreach(int npcKey in npcKeys)
        {
            NPC npc = npcs[npcKey];
            npc.timeLeft = 24f;
            npc.stats.energy = 100f;
        }

        yield return new WaitForSeconds(1f); //add a delay so that menu is shown
        loadingSlider.value = 1f;
        yield return new WaitForSeconds(1f);
        canvas.enabled = false;;
        InputLocker.Unlock();
    }

    IEnumerator reloadWorld()
    {
        var sceneDecoration = GameObject.Find("SceneDecoration"); //get all of the scene decoration

        Destroy(sceneDecoration); //remove scene decoration
    
        builder.BuildScene(); //re-build the scene

        //reset camera position and player position
        Vector3 startPos = camMovement.startingPosition; 

        camMovement.transform.position = startPos;

        player.position = new Vector3(0,0,-0.5f);
        yield break;
    }
}
