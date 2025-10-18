using UnityEngine;

public class House_Generation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject[] housePrefabs;   //array of the prefabs for houses
    public GameObject floorPrefab; //prefab for flooring
    public GameObject outerWallLeftPrefab; //prefab for left outerwall
    public GameObject outerWallRightPrefab; //prefab for right outerwall

    [Header("Build Settings")]
    private int[] houseOrder = {1, 4, 3, 2, 0}; //example array
    private Vector3 floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

    private Vector3[] houseTemps = {House0_ScrObj.startingPosition,
        House1_ScrObj.startingPosition,
        House2_ScrObj.startingPosition,
        House3_ScrObj.startingPosition,
        House4_ScrObj.startingPosition};

    private Vector3 offset = new Vector3(9.36f, 0.0f, 0.0f); //distance between each section

    void OnEnable()
    {
        BuildScene();
    }

    void BuildScene()
    {
        Vector3 housePosition;

        Vector3 wallPosition = new Vector3(-3.04f, -0.18f, 0.0f);
        GameObject leftWall = Instantiate(outerWallLeftPrefab, wallPosition, Quaternion.identity);
        for (int i=0; i<houseOrder.Length; i++)
        {
            if (houseOrder[i] < 0 || houseOrder[i] > 4)
            {
                Debug.LogWarning("House type indentifier out of range: " + houseOrder[i]);
                break;
            }

            //get starting position for the house type
            housePosition = houseTemps[houseOrder[i]];
            Debug.LogWarning("iteration = " + i.ToString() + " offset = " + offset.ToString());
            housePosition += i * offset; //moves position over the offest amount

            //spawn the house prefab linked
            GameObject house = Instantiate(housePrefabs[houseOrder[i]], housePosition, Quaternion.identity);

            //pawn the flooring underneath the house
            GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.Euler(68.064f, 0.0f, 0.0f));

            //move the position for the next set
            floorPosition += offset;
        }
        wallPosition += (houseOrder.Length * offset) + new Vector3(-0.85f, -0.18f, 0.0f);
        GameObject rightWall = Instantiate(outerWallRightPrefab, wallPosition, Quaternion.identity);

        //tell the camera where the side walls are positioned
        CamerMovement camera = Camera.main.GetComponent<CamerMovement>();
        camera.leftWallPos = leftWall.transform;
        camera.rightWallPos = rightWall.transform;
    }
}
