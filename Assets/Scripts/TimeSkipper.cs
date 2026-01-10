using UnityEngine;

public class TimeSkipper : MonoBehaviour
{
    private SceneBuilder builder;
    private CamerMovement camMovement;  //camera script
    public Transform player; 

    void Awake()
    {
        builder = FindFirstObjectByType<SceneBuilder>();
        camMovement = FindFirstObjectByType<CamerMovement>();
        player = FindFirstObjectByType<PlayerMovement>().gameObject.transform;
    }
    public void skipTime(int time)
    {
        reloadWorld();
    }
    private void reloadWorld()
    {
        var sceneDecoration = GameObject.Find("SceneDecoration"); //get all of the scene decoration
        Destroy(sceneDecoration); //remove scene decoration

        builder.BuildScene(); //re-build the scene

        //reset camera position and player position
        Vector3 startPos = camMovement.startingPosition; 

        camMovement.transform.position = startPos;

        player.position = startPos + new Vector3(0, 0, 0.5f);

    }
}
