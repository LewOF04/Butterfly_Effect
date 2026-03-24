using UnityEngine;
public class InputLocker : MonoBehaviour
{
    public static bool IsLocked { get; private set; }
    public static bool IsInteractionLocked{ get; private set; }

    public static void Lock()
    {
        IsLocked = true;
        IsInteractionLocked = true;

        Camera camera = FindFirstObjectByType<Camera>();
        GameObject player = GameObject.Find("Player");
        MonoBehaviour playerMover = player.GetComponent<PlayerMovement>();
        MonoBehaviour cameraMover = camera.GetComponent<CamerMovement>();

        //disable movement
        playerMover.enabled = false;
        cameraMover.enabled = false;
    }

    public static void Unlock()
    {
        IsLocked = false; //allow for clicking
        IsInteractionLocked = false;

        Camera camera = FindFirstObjectByType<Camera>();
        GameObject player = GameObject.Find("Player");
        MonoBehaviour playerMover = player.GetComponent<PlayerMovement>();
        MonoBehaviour cameraMover = camera.GetComponent<CamerMovement>();

        //re-enable movement
        playerMover.enabled = true;
        cameraMover.enabled = true;
    }
}