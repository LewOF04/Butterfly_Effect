using UnityEngine;
using System.Collections;
public class AgentMovement : MonoBehaviour
{
    public NPC self;

    [Header("Sprites")]
    public Sprite standingSprite;
    public Sprite[] walkingSprites; //pointed for walking right
    public SpriteRenderer spriteRenderer;

    [Header("Movement")]
    public float leftLimit;
    public float rightLimit;
    public Transform characterTransform;
    public bool canMove = false;
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 3f;
    [Range(0f, 1f)] public float moveChance = 0.7f;

    [Header("Animation")]
    public float animationRate = 0.18f;
    private Coroutine movementLoop;
    private int walkFrame;
    private float animTimer;

    private void OnEnable()
    {
        if (canMove && movementLoop == null && Application.isPlaying)
        {
            movementLoop = StartCoroutine(movementLoopRoutine());
        }
    }

    private void OnDisable()
    {
        if (movementLoop != null)
        {
            StopCoroutine(movementLoop);
            movementLoop = null;
        }
    }

    public void setCanMove(bool value)
    {
        canMove = value;

        if (!Application.isPlaying) return;

        if (canMove && movementLoop == null)
        {
            movementLoop = StartCoroutine(movementLoopRoutine());
        }
        else if (!canMove && movementLoop != null)
        {
            StopCoroutine(movementLoop);
            movementLoop = null;
            setStandingSprite();
        }
    }

    private IEnumerator movementLoopRoutine()
    {
        while (canMove)
        {
            float idleTime = Random.Range(minIdleTime, maxIdleTime); //decide how long the agent will stand still for
            setStandingSprite();
            yield return new WaitForSeconds(idleTime);

            if (!canMove) yield break;

            //select whether they choose to move at this point
            if (Random.value > moveChance)
            {
                continue;
            }

            //get the position which the agent is going to move to
            float targetX = Random.Range(leftLimit, rightLimit);
            yield return StartCoroutine(moveToX(targetX));
        }

        setStandingSprite();
        movementLoop = null;
    }

    //move the agent to the designated x position
    private IEnumerator moveToX(float targetX)
    {
        targetX = Mathf.Clamp(targetX, leftLimit, rightLimit); //make sure it's within the limit

        //while we can move and the position is not at the target yet
        while (canMove && Mathf.Abs(characterTransform.position.x - targetX) > 0.02f)
        {
            float currentX = characterTransform.position.x;
            float direction = Mathf.Sign(targetX - currentX);

            //change directio of sprite for movement
            Vector3 scale = characterTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            characterTransform.localScale = scale;

            float speed = getMoveSpeed();
            float newX = Mathf.MoveTowards(currentX, targetX, speed * Time.deltaTime);

            //update agent position
            Vector3 pos = characterTransform.position;
            pos.x = newX;
            characterTransform.position = pos;

            updateWalkAnimation(); //update walking sprite

            yield return null;
        }

        setStandingSprite();
    }

    //calculate speed based off of agent condition and dexterity
    private float getMoveSpeed()
    {
        float condition01 = Mathf.Clamp01(self.stats.condition / 100f);
        float dexterity01 = Mathf.Clamp01(self.attributes.dexterity / 100f);

        float combined = (condition01 + dexterity01) * 0.5f;

        return Mathf.Lerp(0.5f, 2.5f, combined);
    }

    //update sprite based on the delta time
    private void updateWalkAnimation()
    {
        animTimer += Time.deltaTime;
        if (animTimer >= animationRate)
        {
            animTimer = 0f;
            walkFrame = (walkFrame + 1) % walkingSprites.Length; //select walking sprite
            spriteRenderer.sprite = walkingSprites[walkFrame]; //updates the sprite
        }
    }

    //change to the non-moving sprite
    private void setStandingSprite()
    {
        animTimer = 0f;
        walkFrame = 0;

        if (spriteRenderer != null && standingSprite != null)
        {
            spriteRenderer.sprite = standingSprite;
        }
    }
}