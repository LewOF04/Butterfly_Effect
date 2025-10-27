using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject floorPrefab; //prefab for flooring

    [Header("Build Settings")]
    //TODO: make house order retrieve values from Data Controller to place
    private int[] houseOrder = {1, 4, 3, 2, 0}; //example array
    private Vector3 floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

    [Header("House ScrObjs")]
    public BuildingType house1Data;
    public BuildingType house2Data;
    public BuildingType house3Data;
    public BuildingType house4Data;
    public BuildingType house5Data;
    public WallType wallData;

    private Vector3[] houseTemps;

    private Vector3 offset = new Vector3(9.36f, 0.0f, 0.0f); //distance between each section

    /*
    void BuildScene()
    {
        Vector3 housePosition;
        houseTemps = {house1Data.startingPosition,
        house2Data.startingPosition,
        house3Data.startingPosition,
        house4Data.startingPosition,
        house5Data.startingPosition};

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
    */
}
