using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ClickHandler : MonoBehaviour
{
    private void Update()
    {
        if(InputLocker.IsLocked == true) return;
        if (Input.GetKeyDown(KeyCode.O)) openOverviewMenu();
    }

    private void openOverviewMenu()
    {
        OverviewMenu overMenu = FindFirstObjectByType<OverviewMenu>();
        Canvas canvas = overMenu.GetComponent<Canvas>();
        Camera camera = FindFirstObjectByType<Camera>();
        overMenu.transform.position = camera.transform.position;
        overMenu.transform.position += new Vector3(0f, 0f, 5f);

        overMenu.displayData();
        canvas.enabled = true; 
    }
}