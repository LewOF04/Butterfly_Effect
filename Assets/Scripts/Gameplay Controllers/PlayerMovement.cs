using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D body;
    public float speed;

    [Header("Sprites")]
    public Sprite standingSprite;
    public Sprite[] walkingSprites;
    public SpriteRenderer spriteRenderer;
    public Transform characterTransform;

    [Header("Animation")]
    public float animationRate = 0.18f;
    private int walkFrame;
    private float animTimer;

    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(xInput) > 0.01f)
        {
            body.linearVelocity = new Vector2(xInput * speed, body.linearVelocity.y);

            //flip sprite based on movement direction
            Vector3 scale = characterTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(xInput);
            characterTransform.localScale = scale;

            updateWalkAnimation();
        }
        else
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            setStandingSprite();
        }
    }

    private void updateWalkAnimation()
    {
        animTimer += Time.deltaTime;
        if (animTimer >= animationRate)
        {
            animTimer = 0f;
            walkFrame = (walkFrame + 1) % walkingSprites.Length;
            spriteRenderer.sprite = walkingSprites[walkFrame];
        }
    }

    private void setStandingSprite()
    {
        animTimer = 0f;
        walkFrame = 0;

        spriteRenderer.sprite = standingSprite;
    }
}
