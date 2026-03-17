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
    public SpriteRenderer skyBackdrop;

    [Header("Assets")]
    public Sprite[] cloudSprites;
    public Sprite[] mediumStones;
    public Sprite[] lightStones;
    public Sprite[] darkStones;
    public Sprite[] lightShrubs;
    public Sprite[] darkShrubs;
    public Sprite[] badTrees;
    public Sprite[] goodTrees;

    [Header("Scenery")]
    public GameObject shrubPrefab;
    public TreeLine rearTrees;
    public TreeLine frontTrees;
    public CloudLine clouds1;
    public CloudLine clouds2;

    [Header("Data References")]
    public List<BuildingType> houseData = new List<BuildingType>();
    public List<NPCType> npcData = new List<NPCType>();
    public WallType wallData;
    public SkipperData timeSkipperData;

    private DataController dataController = DataController.Instance;

    private Vector3 offset; //distance between each section
    private GameObject timeSkipper; //skipper game object 

    public void Awake()
    {
        timeSkipper = Instantiate(skipperPrefab, timeSkipperData.defaultTransform, Quaternion.identity);
        BuildScene();
    }

    private void setSkyColour(float minVal, float maxVal, float val, SpriteRenderer sprite)
    {
        val = Mathf.Clamp(val, minVal, maxVal);
        val = Mathf.InverseLerp(Mathf.Min(minVal, maxVal), Mathf.Max(minVal, maxVal), val);

        Color summerBlue = new Color(0.3922f, 0.6392f, 0.8706f);
        Color cloudyGrey = new Color(0.7725f, 0.8274f, 0.8784f);
        Color gloomyBrown = new Color(0.4588f, 0.3686f, 0.2431f);

        Color colour = val < 0.4f
        ? Color.Lerp(gloomyBrown, cloudyGrey, Mathf.InverseLerp(0f, 0.4f, val))
        : Color.Lerp(cloudyGrey, summerBlue, Mathf.InverseLerp(0.4f, 1f, val));

        sprite.color = colour;
    }

    
    public void BuildScene()
    {
        var npcs = dataController.NPCStorage;
        var buildings = dataController.BuildingStorage;
        var traits = dataController.TraitStorage;
        var NPCBuildingLinks = dataController.NPCBuildingLinks;
        var sceneDecoration = new GameObject("SceneDecoration"); //game object to store scene decoration instantiated at run time

        List<Bounds> buildingBounds = new List<Bounds>();
        List<Bounds> floorBounds = new List<Bounds>();

        float totalCond = 0; //the overall quality of the settlement based upon the total quality of all of the buildings
        int totalSpriteQuality = 0;

        offset = new Vector3(9.36f, 0.0f, 0.0f);
        floorPosition = new Vector3(1.21f, -2.13f, 0.5f);

        Vector3 wallPosition = wallData.defaultTransform;
        GameObject leftWall = Instantiate(wallData.leftPrefab, wallPosition, Quaternion.identity);
        leftWall.transform.SetParent(sceneDecoration.transform);

        timeSkipper.transform.position = timeSkipperData.defaultTransform;

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
            totalCond += cond;
            int spriteNum =
                (cond <= 12.5f) ? 0 :
                (cond <= 25f)   ? 1 :
                (cond <= 37.5f) ? 2 :
                (cond <= 50f)   ? 3 :
                (cond <= 62.5f) ? 4 :
                (cond <= 75f)   ? 5 :
                (cond <= 87.5f) ? 6 : 7;

            buildingsSprite = currHouseData.possibleSprites[spriteNum];
            totalSpriteQuality += spriteNum;

            //set the sprite of the building
            var sr = building.GetComponent<SpriteRenderer>();
            sr.sprite = buildingsSprite;

            //set the scale of the building
            building.gameObject.transform.localScale = currHouseData.scale;
            Bounds buildingBound = sr.bounds;
            buildingBounds.Add(buildingBound);

            //spawn the flooring underneath the house
            GameObject floor = Instantiate(floorPrefab, floorPosition, Quaternion.Euler(68.064f, 0.0f, 0.0f));
            floor.transform.SetParent(sceneDecoration.transform);
            Bounds floorBound = floor.gameObject.GetComponent<SpriteRenderer>().bounds;
            floorBounds.Add(floorBound);

            //set the collider of the building
            var collider = building.gameObject.GetComponent<PolygonCollider2D>();
            collider.pathCount = 1;
            collider.SetPath(0, currHouseData.colliderShape);

            //iterate over all npcs of the building and spawn them
            NPCType currNPCData;
            float buildingLeftEdge = buildingBound.min.x;
            float buildingRightEdge = buildingBound.max.x;
            int iter = 0;
            int inhabitantNum = building.inhabitants.Count;
            foreach (int npcID in building.inhabitants)
            {
                NPC npc = npcs[npcID];
                currNPCData = npcData[npc.spriteType - 1]; 

                float xPos;

                if (inhabitantNum == 1) //if there is only 1 inhabitant, place in the middle
                {
                    xPos = (buildingLeftEdge + buildingRightEdge) * 0.5f;
                }
                else
                {
                    float val = (float) iter / (inhabitantNum - 1);
                    xPos = Mathf.Lerp(buildingLeftEdge, buildingRightEdge, val);
                }

                Vector3 npcPosition = currNPCData.startingPosition;
                npcPosition.x = xPos;

                //set the location to spawn the npc
                npc.gameObject.transform.position = npcPosition;

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
                Sprite[] sprites = currNPCData.getSprites()[npcSpriteNum];
                npcSprite = sprites[0];

                totalCond += npc.stats.condition * 0.75f;

                //set the sprite of the npc
                SpriteRenderer sr1 = npc.GetComponent<SpriteRenderer>();
                sr1.sprite = npcSprite;

                AgentMovement moveInfo = npc.GetComponent<AgentMovement>();
                moveInfo.standingSprite = sprites[0];
                moveInfo.walkingSprites = new Sprite[]{sprites[1], sprites[2], sprites[3]};
                moveInfo.leftLimit = buildingLeftEdge;
                moveInfo.rightLimit = buildingRightEdge; 
                moveInfo.setCanMove(true);

                //set the scale of npc
                npc.gameObject.transform.localScale = currNPCData.scale;

                iter++;
            }

            //move the position for the next set
            floorPosition += offset;
            iteration++;
        }
        wallPosition += (iteration * offset) + new Vector3(-0.96f, 0, 0);
        GameObject rightWall = Instantiate(wallData.rightPrefab, wallPosition, Quaternion.identity);
        rightWall.transform.SetParent(sceneDecoration.transform);

        //calculate the required quality of the outer walls
        int averageSpriteLevel = (int)Math.Round((double)totalSpriteQuality / buildings.Count);
        float averageCond = totalCond / (buildings.Count + (npcs.Count*0.75f));
        var wallSprite = wallData.possibleSprites[averageSpriteLevel];
        var skipperSprite = timeSkipperData.possibleSprites[averageSpriteLevel];

        //=====SKY BOX======
        setSkyColour(0f, 100f, averageCond, skyBackdrop); //set colour of sky
        float dayTime = dataController.worldManager.gameTime % 24f; //day time between 0-24
        float brightness = Mathf.Clamp01(Mathf.Sin((dayTime - 6f) * Mathf.PI / 12f)); //calc sky brightness based on time of day
        Color c = skyBackdrop.color;
        c.a = Mathf.Lerp(0.15f, 1f, brightness); //apply alpha changes
        skyBackdrop.color = c;

        //=====SHRUBS=====
        for(int i = 0; i<buildingBounds.Count; i++)
        {
            Bounds buildingBound = buildingBounds[i];
            Bounds floorBound = floorBounds[i];
            List<XRange> xAreas = GetUncoveredXRanges(floorBound, buildingBound);

            foreach(XRange xrng in xAreas)
            {
                float min = xrng.min;
                float max = xrng.max;

                while(min < max)
                {
                    Sprite thisSprite = PickShrub(averageCond);
                    float zPos = UnityEngine.Random.Range(-0.4f, -0.01f);
                    GameObject shrubInst = Instantiate(shrubPrefab, new Vector3(min, -1.6f, zPos), Quaternion.identity);
                    shrubInst.transform.SetParent(sceneDecoration.transform);

                    shrubInst.GetComponent<SpriteRenderer>().sprite = thisSprite;
                    Bounds spriteBounds = shrubInst.GetComponent<SpriteRenderer>().sprite.bounds;

                    //rescale sprite to rough size
                    float largestSide = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y);
                    float scale = 0.8f / largestSide;
                    shrubInst.transform.localScale = new Vector3(0.4f*scale, 0.4f*scale, 1f);
                    min += 0.2f;
                }
            }
        }

        //set sprite types for background trees
        rearTrees.setTrees(averageCond);
        frontTrees.setTrees(averageCond);

        //set cloud colours for parallax clouds
        clouds1.setClouds(averageCond, dayTime);
        clouds2.setClouds(averageCond, dayTime);

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
    

    private List<XRange> GetUncoveredXRanges(Bounds floorBounds, Bounds buildingBounds)
    {
        List<XRange> ranges = new List<XRange>();

        float floorMin = floorBounds.min.x;
        float floorMax = floorBounds.max.x;
        float buildingMin = buildingBounds.min.x + 0.5f;
        float buildingMax = buildingBounds.max.x - 0.5f;

        if (buildingMin > floorMin) ranges.Add(new XRange(floorMin, Mathf.Min(buildingMin, floorMax)));

        if (buildingMax < floorMax) ranges.Add(new XRange(Mathf.Max(buildingMax, floorMin), floorMax));

        return ranges;
    }

    private Sprite PickShrub(float condition)
    {
        condition = Mathf.Clamp(condition, 0f, 100f);

        float cond01 = condition / 100f;
        float darkShrubWeight   = Mathf.Lerp(1.75f, 2.75f, cond01);
        float lightShrubWeight  = Mathf.Lerp(0.8f, 3.5f, cond01);
        float darkStoneWeight   = Mathf.Lerp(3.0f, 0.8f, cond01);
        float mediumStoneWeight = Mathf.Lerp(1.5f, 1.5f, cond01);
        float lightStoneWeight  = Mathf.Lerp(0.5f, 2f, cond01);

        float total = darkShrubWeight + lightShrubWeight + darkStoneWeight + mediumStoneWeight + lightStoneWeight;

        float roll = UnityEngine.Random.Range(0f, total);

        if ((roll -= darkShrubWeight) < 0f)  return darkShrubs[UnityEngine.Random.Range(0, darkShrubs.Length)];
        if ((roll -= lightShrubWeight) < 0f) return lightShrubs[UnityEngine.Random.Range(0, lightShrubs.Length)];
        if ((roll -= darkStoneWeight) < 0f)  return darkStones[UnityEngine.Random.Range(0, darkStones.Length)];
        if ((roll -= mediumStoneWeight) < 0f) return mediumStones[UnityEngine.Random.Range(0, mediumStones.Length)];
        else return lightStones[UnityEngine.Random.Range(0, lightStones.Length)];
    }
}


internal struct XRange
{
    public float min;
    public float max;

    public XRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Width => max - min;
}