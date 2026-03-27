using UnityEngine;
using TMPro;

public class AgentSpriteHandler : MonoBehaviour
{
    public NPC agent;
    public TextMeshPro nameField;
    public SpriteRenderer playerImage;
    public GameObject deadSpriteGO;
    public GameObject aliveSpriteGO;
    public GameObject speechBubbleGO;
    public NPCType[] npcTypes;
    public NPCType type => npcTypes[agent.spriteType - 1];

    public void showDead()
    {
        deadSpriteGO.SetActive(true);
        aliveSpriteGO.SetActive(false);

        gameObject.GetComponent<AgentMovement>().enabled = false;
        gameObject.GetComponent<SpeechBubbleController>().enabled = false;

        playerImage.sprite = type.getSprites()[7][0];
        nameField.text = agent.fullName;

        speechBubbleGO.SetActive(false);
    } 
    public void showAlive(float leftLimit, float rightLimit)
    {
        deadSpriteGO.SetActive(false);
        aliveSpriteGO.SetActive(true);
        speechBubbleGO.SetActive(true);

        gameObject.GetComponent<AgentMovement>().enabled = true;
        gameObject.GetComponent<SpeechBubbleController>().enabled = true;

        float cond = agent.stats.condition;
        int agentSpriteNum =
            (cond <= 12.5f) ? 0 :
            (cond <= 25f) ? 1 :
            (cond <= 37.5f) ? 2 :
            (cond <= 50f) ? 3 :
            (cond <= 62.5f) ? 4 :
            (cond <= 75f) ? 5 :
            (cond <= 87.5f) ? 6 : 7;
        
        Sprite[] sprites = type.getSprites()[agentSpriteNum];
        aliveSpriteGO.GetComponent<SpriteRenderer>().sprite = sprites[0];

        AgentMovement moveInfo = gameObject.GetComponent<AgentMovement>();
        moveInfo.standingSprite = sprites[0];
        moveInfo.walkingSprites = new Sprite[]{sprites[1], sprites[2], sprites[3]};
        moveInfo.leftLimit = leftLimit;
        moveInfo.rightLimit = rightLimit; 
        moveInfo.setCanMove(true);

        aliveSpriteGO.transform.localScale = type.scale;
    }
    
    public Sprite getBaseSprite()
    {
        if (agent.isAlive)
        {
            float cond = agent.stats.condition;
            int agentSpriteNum =
            (cond <= 12.5f) ? 0 :
            (cond <= 25f) ? 1 :
            (cond <= 37.5f) ? 2 :
            (cond <= 50f) ? 3 :
            (cond <= 62.5f) ? 4 :
            (cond <= 75f) ? 5 :
            (cond <= 87.5f) ? 6 : 7;
        
            return type.getSprites()[agentSpriteNum][0];
        }
        else
        {
           return deadSpriteGO.GetComponent<SpriteRenderer>().sprite; 
        }
    }
}