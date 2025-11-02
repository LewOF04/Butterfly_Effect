using UnityEngine;
using System.Collections.Generic;

public class SceneBuilder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject floorPrefab; //prefab for flooring

    [Header("Build Settings")]
    private Vector3 floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

    [Header("Data References")]
    public BuildingType house1Data;
    public BuildingType house2Data;
    public BuildingType house3Data;
    public BuildingType house4Data;
    public BuildingType house5Data;
    public WallType wallData;

    [Header("Other References")]
    public DataController dataController; //data controller with all information about the scene

    private Vector3 offset = new Vector3(9.36f, 0.0f, 0.0f); //distance between each section

    
    void BuildScene()
    {
        var npcs = dataController.NPCStorage;
        var buildings = dataController.BuildingStorage;
        var traits = dataController.TraitStorage;
        var NPCBuildingLinks = dataController.NPCBuildingLinks;
        
        
        Vector3 wallPosition = new Vector3(-3.04f, -0.18f, 0.0f);
        GameObject leftWall = Instantiate(wallData.leftPrefab, wallPosition, Quaternion.identity);
        //TODO: instantiate sprite onto left wall

        int iteration = 0;
        BuildingType currHouseData;
        Vector3 housePosition;
        foreach (KeyValuePair<string, Building> entry in buildings) //iterate over all buildings
        {
            Building building = entry.Value;
            string id = entry.Key;

            //get the data for the house type of this current building
            switch (building.buildingType)
            {
                case 1:
                    currHouseData = house1Data;
                    break;

                case 2:
                    currHouseData = house2Data;
                    break;

                case 3:
                    currHouseData = house3Data;
                    break;

                case 4:
                    currHouseData = house4Data;
                    break;

                default:
                    currHouseData = house5Data;
                    break;
            }

            housePosition = currHouseData.defaultPosition;
            housePosition += iteration * offset; //moves position over the offset amount

            //move the house to the designated position
            building.gameObject.transform.position = housePosition;

            //set the sprite of the building
            Sprite buildingsSprite;
            float c = building.condition;
            int idx =
                (c <= 12.5f) ? 0 :
                (c <= 25f)   ? 1 :
                (c <= 37.5f) ? 2 :
                (c <= 50f)   ? 3 :
                (c <= 62.5f) ? 4 :
                (c <= 75f)   ? 5 :
                (c <= 87.5f) ? 6 : 7;

            buildingsSprite = currHouseData.possibleSprites[idx];

            var sr = building.GetComponent<SpriteRenderer>();
            sr.sprite = buildingsSprite;

            building.gameObject.transform.localScale = currHouseData.scale;

            //spawn the flooring underneath the house
            GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.Euler(68.064f, 0.0f, 0.0f));

            //move the position for the next set
            floorPosition += offset;
            iteration++;
        }
        wallPosition += (iteration * offset) + new Vector3(-0.85f, -0.18f, 0.0f);
        GameObject rightWall = Instantiate(wallData.rightPrefab, wallPosition, Quaternion.identity);
        //TODO: instantiate sprite onto rightWall

        //tell the camera where the side walls are positioned
        CamerMovement camera = Camera.main.GetComponent<CamerMovement>();
        camera.leftWallPos = leftWall.transform;
        camera.rightWallPos = rightWall.transform;
    }
    
}
