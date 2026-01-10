using UnityEngine;
using System;
using System.Collections.Generic;

public class SceneBuilder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Prefabs")]
    public GameObject floorPrefab; //prefab for flooring
    public GameObject skipperPrefab; //prefab for time skipper

    [Header("Build Settings")]
    private Vector3 floorPosition;

    [Header("Data References")]
    public List<BuildingType> houseData = new List<BuildingType>();
    public List<NPCType> npcData = new List<NPCType>();
    public WallType wallData;
    public SkipperData timeSkipperData;

    private DataController dataController = DataController.Instance;

    private Vector3 offset; //distance between each section

    public void Awake()
    {
        BuildScene();
    }

    
    public void BuildScene()
    {
        var npcs = dataController.NPCStorage;
        var buildings = dataController.BuildingStorage;
        var traits = dataController.TraitStorage;
        var NPCBuildingLinks = dataController.NPCBuildingLinks;
        var sceneDecoration = new GameObject("SceneDecoration"); //game object to store scene decoration instantiated at run time


        int totalQuality = 0; //the overall quality of the settlement based upon the total quality of all of the buildings

        offset = new Vector3(9.36f, 0.0f, 0.0f);
        floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

        Vector3 wallPosition = wallData.defaultTransform;
        GameObject leftWall = Instantiate(wallData.leftPrefab, wallPosition, Quaternion.identity);
        leftWall.transform.SetParent(sceneDecoration.transform);

        Vector3 skipperPosition = timeSkipperData.defaultTransform;
        GameObject timeSkipper = Instantiate(skipperPrefab, skipperPosition, Quaternion.identity);
        timeSkipper.transform.SetParent(sceneDecoration.transform);

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
            float cond = building.condition;
            int spriteNum =
                (cond <= 12.5f) ? 0 :
                (cond <= 25f)   ? 1 :
                (cond <= 37.5f) ? 2 :
                (cond <= 50f)   ? 3 :
                (cond <= 62.5f) ? 4 :
                (cond <= 75f)   ? 5 :
                (cond <= 87.5f) ? 6 : 7;

            buildingsSprite = currHouseData.possibleSprites[spriteNum];
            totalQuality += spriteNum;

            //set the sprite of the building
            var sr = building.GetComponent<SpriteRenderer>();
            sr.sprite = buildingsSprite;

            //set the scale of the building
            building.gameObject.transform.localScale = currHouseData.scale;

            //spawn the flooring underneath the house
            GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.Euler(68.064f, 0.0f, 0.0f));
            floor.transform.SetParent(sceneDecoration.transform);

            //set the collider of the building
            var collider = building.gameObject.GetComponent<PolygonCollider2D>();
            collider.pathCount = 1;
            collider.SetPath(0, currHouseData.colliderShape);

            //iterate over all npcs of the building and spawn them
            NPCType currNPCData;
            Vector3 buildingOffset = new Vector3(housePosition.x, 0, 0);
            foreach (int npcID in building.inhabitants)
            {
                NPC npc = npcs[npcID];
                currNPCData = npcData[npc.spriteType - 1]; 
                Vector3 npcPosition = currNPCData.startingPosition + buildingOffset;

                //set the location to spawn the npc
                npc.gameObject.transform.position = npcPosition;
                buildingOffset += new Vector3(1.0f, 0.0f, 0.0f);

                //get the condition and sprite of the npc
                Sprite npcSprite;
                float cond1 = npc.stats.condition;
                int npcSpriteNum =
                    (cond1 <= 12.5f) ? 0 :
                    (cond1 <= 25f) ? 1 :
                    (cond1 <= 37.5f) ? 2 :
                    (cond1 <= 50f) ? 3 :
                    (cond1 <= 62.5f) ? 4 :
                    (cond1 <= 75f) ? 5 :
                    (cond1 <= 87.5f) ? 6 : 7;
                npcSprite = currNPCData.possibleSprites[npcSpriteNum];

                //set the sprite of the npc
                var sr1 = npc.GetComponent<SpriteRenderer>();
                sr1.sprite = npcSprite;

                //set the scale of npc
                npc.gameObject.transform.localScale = currNPCData.scale;
            }

            //move the position for the next set
            floorPosition += offset;
            iteration++;
        }
        wallPosition += (iteration * offset) + new Vector3(-0.96f, 0, 0.0f);
        GameObject rightWall = Instantiate(wallData.rightPrefab, wallPosition, Quaternion.identity);
        rightWall.transform.SetParent(sceneDecoration.transform);

        //calculate the required quality of the outer walls
        int averageQuality = (int)Math.Round((double)totalQuality / buildings.Count);
        var wallSprite = wallData.possibleSprites[averageQuality];
        var skipperSprite = timeSkipperData.possibleSprites[averageQuality];

        //apply sprite to the time skipper post
        var skipperSR = timeSkipper.GetComponent<SpriteRenderer>();
        skipperSR.sprite = skipperSprite;
        timeSkipper.gameObject.transform.localScale = timeSkipperData.scale;

        //set collider for time skipper post
        var skipCollider = timeSkipper.gameObject.GetComponent<PolygonCollider2D>();
        skipCollider.pathCount = 1;
        skipCollider.SetPath(0, timeSkipperData.colliderShape);

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
