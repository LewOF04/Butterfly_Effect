using UnityEngine;

public class TimeSkipper : MonoBehaviour
{
    private DataController dataController = DataController.Instance;
    public  SceneBuilder builder;
    public void reloadWorld()
    {
        Debug.Log("Reloading");
        //builder.BuildScene();
    }
}
