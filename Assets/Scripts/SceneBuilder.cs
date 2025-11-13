using UnityEngine;
using System;
using System.Collections.Generic;

public class SceneBuilder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject floorPrefab; //prefab for flooring

    [Header("Build Settings")]
    private Vector3 floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

    [Header("Data References")]
    public List<BuildingType> houseData = new List<BuildingType>();
    public List<NPCType> npcData = new List<NPCType>();
    public WallType wallData;


    private DataController dataController = DataController.Instance;

    private Vector3 offset = new Vector3(9.36f, 0.0f, 0.0f); //distance between each section

    public void Awake()
    {
        BuildScene();
    }

    
    void BuildScene()
    {
        var npcs = dataController.NPCStorage;
        var buildings = dataController.BuildingStorage;
        var traits = dataController.TraitStorage;
        var NPCBuildingLinks = dataController.NPCBuildingLinks;


        int totalQuality = 0; //the overall quality of the settlement based upon the total quality of all of the buildings
        
        
        Vector3 wallPosition = new Vector3(-3.04f, -0.18f, 0.0f);
        GameObject leftWall = Instantiate(wallData.leftPrefab, wallPosition, Quaternion.identity);

        int iteration = 0;
        BuildingType currHouseData;
        Vector3 housePosition;
        foreach (KeyValuePair<int, Building> entry in buildings) //iterate over all buildings
        {
            //retrieve the current building
            Building building = entry.Value;
            int id = entry.Key;

            //get the data for the house type of this current building
            currHouseData = houseData[building.buildingType - 1];

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
            totalQuality += idx;

            var sr = building.GetComponent<SpriteRenderer>();
            sr.sprite = buildingsSprite;

            building.gameObject.transform.localScale = currHouseData.scale;

            //spawn the flooring underneath the house
            GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.Euler(68.064f, 0.0f, 0.0f));

            //iterate over all npcs of the building and spawn them
            var npcPosition = housePosition - (iteration * offset);
            NPCType currNPCData;
            foreach (int npcID in building.inhabitants)
            {
                NPC npc = npcs[npcID];
                currNPCData = npcData[npc.spriteType - 1];

                npc.gameObject.transform.position = npcPosition;
                npcPosition += new Vector3(0.1f, 0.0f, 0.0f);

                Sprite npcSprite;
                float c1 = npc.stats.condition;
                int idx1 =
                    (c1 <= 12.5f) ? 0 :
                    (c1 <= 25f) ? 1 :
                    (c1 <= 37.5f) ? 2 :
                    (c1 <= 50f) ? 3 :
                    (c1 <= 62.5f) ? 4 :
                    (c1 <= 75f) ? 5 :
                    (c1 <= 87.5f) ? 6 : 7;
                npcSprite = currNPCData.possibleSprites[idx1];

                var sr1 = npc.GetComponent<SpriteRenderer>();
                sr1.sprite = npcSprite;

                npc.gameObject.transform.localScale = currNPCData.scale;
            }

            //move the position for the next set
            floorPosition += offset;
            iteration++;
        }
        wallPosition += (iteration * offset) + new Vector3(-0.85f, -0.18f, 0.0f);
        GameObject rightWall = Instantiate(wallData.rightPrefab, wallPosition, Quaternion.identity);

        //calculate the required quality of the outer walls
        int averageQuality = (int)Math.Round((double)totalQuality / buildings.Count);
        Debug.Log("Average Quality = " + averageQuality.ToString());
        var wallSprite = wallData.possibleSprites[averageQuality];

        //apply the sprites to the right and left walls
        var rightWallSR = rightWall.GetComponent<SpriteRenderer>();
        var leftWallSR = leftWall.GetComponent<SpriteRenderer>();
        rightWallSR.sprite = wallSprite;
        leftWallSR.sprite = wallSprite;
        leftWallSR.flipX = true;
        leftWall.gameObject.transform.localScale = wallData.scale;
        rightWall.gameObject.transform.localScale = wallData.scale;

        //tell the camera where the side walls are positioned
        CamerMovement camera = Camera.main.GetComponent<CamerMovement>();
        camera.leftWallPos = leftWall.transform;
        camera.rightWallPos = rightWall.transform;
    } 
    
}
