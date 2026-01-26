using UnityEngine;

public class CamerMovement : MonoBehaviour
{
    public Vector3 startingPosition;
    public Transform playerTransform;
    public Transform leftWallPos;
    public Transform rightWallPos;
    public GameObject backDrop;
    public Vector3 wallStartingPosition;

    private Vector3 leftLimit;
    private Vector3 rightLimit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = startingPosition;
        leftLimit = startingPosition;
        rightLimit = rightWallPos.position - new Vector3(3.125f, 0, 0);
        backDrop.transform.position = new Vector3(playerTransform.position.x + wallStartingPosition.x, wallStartingPosition.y, wallStartingPosition.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (playerTransform.position.x > leftLimit.x && playerTransform.position.x < rightLimit.x)
        {
            transform.position = new Vector3(playerTransform.position.x, startingPosition.y, startingPosition.z);
            backDrop.transform.position = new Vector3(playerTransform.position.x + wallStartingPosition.x, wallStartingPosition.y, wallStartingPosition.z);
        }
    }
}
